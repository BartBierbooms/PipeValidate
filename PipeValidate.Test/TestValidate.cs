using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeValidate.Test.TestData;
using Piping;

namespace PipeValidate.Test
{
    [TestClass]
    public class TestValidate
    {

        [TestMethod]
        public void Test_Validate_Initialize()
        {
            var pipeline = Pipe.Init(() => new Result(), new Validator<HttpRequestContext, Result>());

            var pipelineResult = (Validator<HttpRequestContext, Result>)pipeline(new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser()));
            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Some);
        }

        [TestMethod]
        public void Test_Validate_Then_WithInvalid()
        {

            var pipeline = Pipe.Init(() => new Result(), new Validator<HttpRequestContext, Result>())
                .Then(resp => ValidateFields.AddInvalidAge(resp));

            var pipelineResult = (Validator<HttpRequestContext, Result>)pipeline(new HttpRequestContext());

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.IsValid == false);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 1);
        }

        [TestMethod]
        public void Test_Validate_Then_With_Exception()
        {
            var pipeline = Pipe.Init(() => new Result(), new Validator<HttpRequestContext, Result>())
                .Then((HttpRequestContext a) => ValidateFields.ValidationThatThrowsAnException());

            var pipelineResult = (Validator<HttpRequestContext, Result>)pipeline(new HttpRequestContext());

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Exception);
            Assert.IsTrue(pipelineResult.ExceptionVal.Message == ValidateFields.ExceptionMessage);
        }

        [TestMethod]
        public void Test_Validate_Then_With_Exception_StopsPipe()
        {
            var pipeline = Pipe.Init(() => new Result(), new Validator<HttpRequestContext, Result>())
                .Then((HttpRequestContext a) => ValidateFields.ValidationThatThrowsAnException())
                .Then(resp => ValidateFields.AddInvalidAge(resp));

            var pipelineResult = (Validator<HttpRequestContext, Result>)pipeline(new HttpRequestContext());

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Exception);
            Assert.IsTrue(pipelineResult.ExceptionVal.Message == ValidateFields.ExceptionMessage);
            Assert.IsTrue(pipelineResult.SupplementVal == null);

        }

        [TestMethod]
        public void Test_Validate_Join_Two_ValidatesPipeLines()
        {
            var testData = new PersonData("ikke@ikke.nl", "ikke", "02034567887", 18, true);
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());
            int nr = 1;
            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsAuthenticated(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person" + nr, testData));

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal));

            var pipelineBoth = validateContext.Join(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person" + nr)), validateFields);

            var pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Some);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 0);


            testData = new PersonData("", "ikke", "02034567887", 18, true);
            nr++;
            pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 1);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == ValidateFields.PersonNoValidEmail));

            testData = new PersonData("", "", "02034567887", 18, true);
            nr++;
            pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);


            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 2);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == ValidateFields.PersonNoValidEmail));
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == ValidateFields.PersonNoValidName));

        }

        [TestMethod]
        public void Test_Validate_JoinTwoPipelines_Stop_If_First_IsInvalid()
        {
            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);
            var testData = new PersonData("ikke@ikke.nl", "ikke", "02034567887", 18, true);
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsEmployee_WhichFails(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person", testData)); ;

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal));


            var pipelineBoth = validateContext.JoinIfValid(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person")), validateFields);

            var pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 1);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == HttpRequestContext.inValidRole));

        }

        [TestMethod]
        public void Test_Validate_JoinTwoPipelines_Continue_If_First_Isvalid()
        {
            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);

            var testData = new PersonData("ikke@ikke.nl", "ikke", "02034567887", 18, true);
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsAuthenticated(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person", testData)); ;

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal));


            var pipelineBoth = validateContext.JoinIfValid(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person")), validateFields);

            var pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Some);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 0);

        }

        [TestMethod]
        public void Test_Validate_WithIif()
        {
            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);
            int nr = 1;

            var testData = new PersonData("", "ikke", "02034567887", 18, true);
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsAuthenticated(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person" + nr, testData));

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal))
                .Iff(valAndResult => valAndResult.Val.Person.IsStudent)
                    .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .EndIff();


            var pipelineBoth = validateContext.JoinIfValid(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person" + nr)), validateFields);

            var pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 1);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(m => m == ValidateFields.PersonNoValidEmail));

            //IiF predicate is falsly
            testData = new PersonData("", "ikke", "02034567887", 18, false);
            nr++;
            pipelineResult = (Validator<ValidateFields, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Some);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 0);
            Assert.IsTrue(pipelineResult.Val.Person.Email == "");

        }

        [TestMethod]
        public void Test_Validate_Join_Three_ValidatesPipeLines()
        {
            var testData = new PersonData("ikke@ikke.nl", "ikke", "02034567887", 18, true) { Address = new Address("mien street") };
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());
            int nr = 1;

            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);
            var initReferenceValidate = ValidatorExt.InitPipe<ValidateReferences, Result>(out var referenceValidator);

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsAuthenticated(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person" + nr, testData));

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal));

            var validateReferences = Pipe.Init(initReferenceValidate, referenceValidator)
                .Then(valAndResult => valAndResult.Val.AddInvalidAddress(valAndResult.SupplementVal));

            var pipelineBoth = validateContext
                .JoinIfValid(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person" + nr)), validateFields)
                .Join(valflds => new ValidateReferences(valflds.Person), validateReferences);

            var pipelineResult = (Validator<ValidateReferences, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Some);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 0);


            testData = new PersonData("", "ikke", "02034567887", 18, true) { Address = new Address("") };
            nr++;
            pipelineResult = (Validator<ValidateReferences, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 2);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == ValidateFields.PersonNoValidEmail));
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(x => x == ValidateReferences.InValidAddress));

        }


        [TestMethod]
        public void Test_Validate_Join_Three_ValidatesPipeLines_StopOnFirst_Invalid()
        {
            var testData = new PersonData("ikke@ikke.nl", "ikke", "02034567887", 18, true) { Address = new Address("mien street") };
            var httpRequest = new HttpRequestContext(HttpContextHelper.SetUpHttpContextAccessor(), HttpContextHelper.GetUser());

            var initContextValidate = ValidatorExt.InitPipe<HttpRequestContext, Result>(out var contextValidator);
            var initFieldsValidate = ValidatorExt.InitPipe<ValidateFields, Result>(out var fieldsValidator);
            var initReferenceValidate = ValidatorExt.InitPipe<ValidateReferences, Result>(out var referenceValidator);

            var validateContext = Pipe.Init(initContextValidate, contextValidator)
                .Then(contextAndResult => contextAndResult.Val.ValidateIsEmployee_WhichFails(contextAndResult.SupplementVal))
                .Then(context => HttpContextHelper.SetObjectAsJson(context.ContextAccessor, "person", testData));

            var validateFields = Pipe.Init(initFieldsValidate, fieldsValidator)
                .Then(valAndResult => valAndResult.Val.ValidateEmail(valAndResult.SupplementVal))
                .Then(valAndResult => valAndResult.Val.ValidatePerson(valAndResult.SupplementVal));

            var validateReferences = Pipe.Init(initReferenceValidate, referenceValidator)
                .Then(valAndResult => valAndResult.Val.AddInvalidAddress(valAndResult.SupplementVal));

            var pipelineBoth = validateContext
                .JoinIfValid(cnt => new ValidateFields(HttpContextHelper.GetObjectFromJson<PersonData>(cnt.ContextAccessor, "person" )), validateFields)
                .Join(valflds => new ValidateReferences(valflds.Person), validateReferences);

            var pipelineResult = (Validator<ValidateReferences, Result>)pipelineBoth(httpRequest);

            Assert.IsTrue(pipelineResult.GetOptionType == OptionType.Validation);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Count == 1);
            Assert.IsTrue(pipelineResult.SupplementVal.Messages.Any(m => m == HttpRequestContext.inValidRole));

        }

    }
}
