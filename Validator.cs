using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Piping;

namespace PipeValidate
{

	public static class ValidatorExt
	{
        /// <summary>
        /// Helper function to initialize a pipe with an instance of Validator class in stead of the default Option classes implementation.
        /// </summary>
        /// <typeparam name="TI">The type of the object that is passed into the pipeline delegate when invoked</typeparam>
        /// <typeparam name="TV">The type of the object that is returned as the Supplemented value of the IValueAndSupplement interface</typeparam>
        /// <param name="validator">An instance of the Validator<TI, TV> class</param>
        /// <returns>Func delegate that returns an object of type TV.</returns>
        public static Func<TV> InitPipe<TI, TV>(out Validator<TI, TV> validator)
			where TV : IValidatorResults, new()
			where TI : new()
		{
			validator = new Validator<TI, TV>();
			return () => new TV();
		}

		/// <summary>
		/// Combines two Validate pipelines, even if the first pipeline is invalid.
		/// </summary>
		/// <typeparam name="TI">The type of the object that is passed into the pipeline delegate when invoked.</typeparam>
		/// <typeparam name="TV">First pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TW">Second pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TR">The resulting object that is the Supplemented value of the returned ToValueSupplementValue object. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <typeparam name="TS">The supplemented value of the first pipeline. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <param name="firstValidatorPipe">The first validate pipelines</param>
		/// <param name="invokeeSecondPipeLine">Func delegate that returns the second validate pipeline.</param>
		/// <param name="secondValidatorPipe">Validator type of the resulting pipeline.</param>
		/// <returns>An object of type ToValueSupplementValue (Func delegate that returns an object that implements the IValueAndSupplement interface when invoked with an object of type TI.</returns>
        public static Pipe.ToValueSupplementValue<TI, TW, TR> Join<TI, TV, TW, TR, TS>(
			this Pipe.ToValueSupplementValue<TI, TV, TS> firstValidatorPipe, Func<TV, TW> invokeeSecondPipeLine,
			Pipe.ToValueSupplementValue<TW, TW, TR> secondValidatorPipe)
			where TI : new()
			where TV : new()
			where TW : new()
			where TR : IValidatorResults, new()
			where TS : IValidatorResults, new()
		{
			return firstValidatorPipe.Transform(invokeeSecondPipeLine,
				secondValidatorPipe,
				(src, ret) => ret.Messages.AddRange(src.Messages));
		}

		/// <summary>
		/// Combines two Validate pipelines, even if the first pipeline is invalid.
		/// </summary>
		/// <typeparam name="TI">The type of the object that is passed into the pipeline delegate when invoked.</typeparam>
		/// <typeparam name="TV">First pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TW">Second pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TR">The resulting object that is the Supplemented value of the returned ToValueSupplementValue object. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <typeparam name="TS">The supplemented value of the first pipeline. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <param name="firstValidatorPipe">The first validate pipelines</param>
		/// <param name="invokeeSecondPipeLine">Func delegate that returns the second validate pipeline.</param>
		/// <param name="secondValidatorPipe">Validator type of the resulting pipeline.</param>
		/// <returns>An object of type ToValueSupplementValue (Func delegate that returns an object that implements the IValueAndSupplement interface when invoked with an object of type TI.</returns>
		public static Pipe.ToValueSupplementValue<TI, TW, TR> Join<TI, TV, TW, TR, TS>(
			this Pipe.ToValueSupplementValue<TI, TV, TS> firstValidatorPipe, TW invokeeSecondPipeLine,
			Pipe.ToValueSupplementValue<TW, TW, TR> secondValidatorPipe)
			where TI : new()
			where TV : new()
			where TW : new()
			where TR : IValidatorResults, new()
			where TS : IValidatorResults, new()
		{
			return firstValidatorPipe.Then((TS _) => invokeeSecondPipeLine,
				secondValidatorPipe,
				(src, ret) => ret.Messages.AddRange(src.Messages));
		}

		/// <summary>
		/// Combines two Validate pipelines, but only if the first pipeline is valid.
		/// </summary>
		/// <typeparam name="TI">The type of the object that is passed into the pipeline delegate when invoked.</typeparam>
		/// <typeparam name="TV">First pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TW">Second pipeline: The type of the source object that is the Val of the source IValueAndSupplement interface.</typeparam>
		/// <typeparam name="TR">The resulting object that is the Supplemented value of the returned ToValueSupplementValue object. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <typeparam name="TS">The supplemented value of the first pipeline. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
		/// <param name="firstValidatorPipe">The first validate pipelines</param>
		/// <param name="invokeeSecondPipeLine">Func delegate that returns the second validate pipeline.</param>
		/// <param name="secondValidatorPipe">Validator type of the resulting pipeline.</param>
		/// <returns>An object of type ToValueSupplementValue (Func delegate that returns an object that implements the IValueAndSupplement interface when invoked with an object of type TI.</returns>
        public static Pipe.ToValueSupplementValue<TI, TW, TR> JoinIfValid<TI, TV, TW, TR, TS>(
			this Pipe.ToValueSupplementValue<TI, TV, TS> firstValidatorPipe, Func<TV, TW> invokeeSecondPipeLine,
			Pipe.ToValueSupplementValue<TW, TW, TR> secondValidatorPipe)
			where TI : new()
			where TV : new()
			where TW : new()
			where TR : IValidatorResults, new()
			where TS : IValidatorResults, new()
		{
			TS input = new TS();

			Pipe.ToValueSupplementValue<TW, TW, TR> Second()
			{
				if (input.Messages.Count == 0)
				{
					return secondValidatorPipe;
				}

				return Pipe.Init(() => new TR(), new Validator<TW, TR>());
			}

			return firstValidatorPipe.Transform(x =>
				{
					input = x.SupplementVal;
					return invokeeSecondPipeLine(x.Val);
				}, Second,
				(TS src, TR ret) => ret.Messages.AddRange(src.Messages));
		}

        /// <summary>
        /// Wraps the result value of an pipe into a Validator instance.
        /// The ValidationResult property of the Validator class indicates if the pipe was successful, non-successful with broken validations, executed with errors of null values.
        /// </summary>
        /// <typeparam name="TV">The type of the source object that is the Val value of the IValueAndSupplement interface.</typeparam>
        /// <typeparam name="TS">The type of the source object that is the Supplemented value the IValueAndSupplement interface. The supplemented value implements the IValidatorResults interface with broken validations.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>A validator instance.</returns>
        public static Validator<TV, TS> Return<TV, TS>(this IValueAndSupplement<TV, TS> source)
			where TV : new()
			where TS : IValidatorResults, new()
		{
			return new Validator<TV, TS>(source, new PipeOption(true, null), source.SupplementVal.Messages);
		}
	}

	public enum ValidationResult
	{
		None = 0,
		Some = 1,
		SomeInvalid = 2,
		Exception = 3,
	}

	/// <summary>
    /// Validator class overrides the default Option implementation in a pipeline.
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    /// <typeparam name="TS"></typeparam>
	public class Validator<TV, TS> : PipeBase<TV, TS>
		where TV : new()
		where TS : new()
	{
		protected internal bool? ConditionMet;
		protected internal readonly IList<IValueAndSupplementExtension> ExtensionInterfaces;
		protected TS GetSupplement { get; }
		protected TV GetValue { get; }

		private Exception ex;
		private ValidationResult ValidatorType { get; }

		public List<string> Messages => ExtractMessages(GetValue, GetSupplement);

		public OptionType GetOptionType
		{
			get
			{
				switch (ValidatorType)
				{
					case ValidationResult.Some:
						return OptionType.Some;
					case ValidationResult.None:
						return OptionType.None;
					case ValidationResult.SomeInvalid:
						return OptionType.Validation;
					case ValidationResult.Exception:
						return OptionType.Exception;
					default:
						return OptionType.None;
				}
			}
		}

		protected override bool? ConditionIsMet => ConditionMet;
		protected override IList<IValueAndSupplementExtension> PipebaseExtensions => ExtensionInterfaces;

		public override bool TestInputIsInvariant<TI>(TI input)
		{
			return false;
		}

		public override bool ValidateIsValid<TA, TB>(PipeBase<TA, TB> source, out PipeBase<TA, TB> pipeBaseValidated)
		{
			var validateSource = source as Validator<TA, TB>;
			pipeBaseValidated = source;

			if (validateSource != null && validateSource.ValidatorType == ValidationResult.SomeInvalid)
			{
				return false;
			}

			if (validateSource != null)
			{
				var msgs = ExtractMessages(source.Val, source.SupplementVal);
				if (msgs.Count > 0)
				{
					var pipeOption = (IPipeOption) source;
					pipeBaseValidated = new Validator<TA, TB>(new ValueAndSupplement<TA, TB>(source.Val, source.SupplementVal), pipeOption, msgs);
					return false;
				}
			}

			return true;
		}

		public override PipeBase<TA, TB> CreateException<TA, TB>(Exception ex, IPipeOption pipeOption)
		{
			return new Validator<TA, TB>(ex, pipeOption, null);
		}

		public override PipeBase<TV, TS> CreateNone(IPipeOption pipeOption)
		{
			return new Validator<TV, TS>(pipeOption, ValidationResult.None, null);
		}

		public override PipeBase<TV, TS> CreateSome(IValueAndSupplement<TV, TS> val, IPipeOption pipeOption)
		{
			var msgs = ExtractMessages(GetValue, GetSupplement);
			return new Validator<TV, TS>(val, pipeOption, msgs);
		}

		public override PipeBase<TV, TS> CreateValidation(IValueAndSupplement<TV, TS> val, IPipeOption pipeOption, System.ComponentModel.DataAnnotations.ValidationResult result)
		{
			var msgs = ExtractMessages(GetValue, GetSupplement);
			return new Validator<TV, TS>(val, pipeOption, msgs);
		}

		public override PipeBase<TA, TB> WrapPipeLineResult<TA, TB>(TA value, TB supplementedValue, IPipeOption pipeOption)
		{
			var msgs = new List<string>();
			if (value == null && supplementedValue == null)
			{
				return new Validator<TA, TB>(new InvalidOperationException("Null values for value and supplemented value are not allowed!"), pipeOption, new List<string>());
			}

			if (supplementedValue == null)
			{
				return new Validator<TA, TB>(new InvalidOperationException("Null values for supplemented value is not allowed!"), pipeOption, new List<string>());
			}

			try
			{
				ExecuteExtensions(pipeOption, value);
				ExecuteExtensions(pipeOption, supplementedValue);
				msgs = ExtractMessages(value, supplementedValue);
			}
			catch (Exception exExtensions)
			{
				return new Validator<TA, TB>(exExtensions, pipeOption, ExtractMessages(value, supplementedValue));
			}

			return new Validator<TA, TB>(new ValueAndSupplement<TA, TB>(value, supplementedValue), pipeOption, msgs);
		}

		private List<string> ExtractMessages<TA, TB>(TA value, TB supplementedValue)
		{
			var retmsgs = new List<string>();
			if (value != null && value is IValidatorResults ret && ret.Messages.Any())
			{
				retmsgs.AddRange(ret.Messages);
			}

			if (supplementedValue != null && supplementedValue is IValidatorResults retSupp && retSupp.Messages.Any())
			{
				retmsgs.AddRange(retSupp.Messages);
			}

			return retmsgs;
		}

		public override PipeBase<TI, TA> WrapPipeLineResult<TI, TA>(Func<TA> init, TI value, Piping.PipeOption pipeOption, bool isInit = false)
		{
			try
			{
				TA nwevalue = init();
				if (isInit && value.Equals(nwevalue))
				{
					return new Validator<TI, TA>(new InvalidOperationException("The Init function must create a new instance of an object"), pipeOption, null);
				}

				ExecuteExtensions(pipeOption, value);
				ExecuteExtensions(pipeOption, nwevalue);

				if (nwevalue == null)
				{
					return new Validator<TI, TA>(PipeOption.PipeOptionNone, ValidationResult.None, null);
				}

				var msgs = ExtractMessages(value, nwevalue);
				return new Validator<TI, TA>(new ValueAndSupplement<TI, TA>(value, nwevalue), pipeOption, msgs);
			}
			catch (Exception ex1)
			{
				return new Validator<TI, TA>(ex1, PipeOption.PipeOptionNone, null);
			}
		}

		public override bool ContinuePipeLineEntry<TA, TB, TC, TD>(
			IValueAndSupplement<TA, TB> pipeInput,
			out PipeBase<TC, TD> wrapInputIntoPipeBaseWhenBreak,
			out IPipeOption pipeOption)
		{
			if (pipeInput == null)
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBaseWhenBreak = new Validator<TC, TD>(pipeOption, ValidationResult.None, null);
				return false;
			}

			if (pipeInput.GetType() != typeof(Validator<TA, TB>))
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBaseWhenBreak = new Validator<TC, TD>(new InvalidCastException(), pipeOption, null);
				return false;
			}

			pipeOption = (IPipeOption) pipeInput;
			var validatorType = ((Validator<TA, TB>) pipeInput).ValidatorType;

			if (validatorType == ValidationResult.None)
			{
				wrapInputIntoPipeBaseWhenBreak = new Validator<TC, TD>(pipeOption, ValidationResult.None, null);
				return false;
			}

			if (validatorType == ValidationResult.Exception)
			{
				wrapInputIntoPipeBaseWhenBreak = new Validator<TC, TD>(((Validator<TA, TB>) pipeInput).ExceptionVal, pipeOption, null);
				return false;
			}

			//Because of the out signature we need to instantiate a wrapInputIntoPipeBase, but it will be ignored because we return true, meaning don't break
			wrapInputIntoPipeBaseWhenBreak = new Validator<TC, TD>(new ValueAndSupplement<TC, TD>(default(TC), default(TD)), pipeOption, new List<string>());
			return true;
		}

		public override bool ContinuePipeLineEntry<TB, TA>(
			IValueAndSupplement<TA, TB> pipeInput,
			out PipeBase<TB, TA> wrapInputIntoPipeBase,
			out IPipeOption pipeOption)
		{
			if (pipeInput == null)
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBase = new Validator<TB, TA>(pipeOption, ValidationResult.None, null);
				return false;
			}

			if (pipeInput.GetType() != typeof(Validator<TA, TB>))
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBase = new Validator<TB, TA>(new InvalidCastException(), pipeOption, null);
				return false;
			}

			pipeOption = (IPipeOption) pipeInput;
			var validatorType = ((Validator<TA, TB>) pipeInput).ValidatorType;

			if (validatorType == ValidationResult.Exception)
			{
				wrapInputIntoPipeBase = new Validator<TB, TA>(((Validator<TA, TB>) pipeInput).ExceptionVal, pipeOption, null);
				return false;
			}

			wrapInputIntoPipeBase = new Validator<TB, TA>(new ValueAndSupplement<TB, TA>(pipeInput.SupplementVal, pipeInput.Val), pipeOption, ExtractMessages(pipeInput.Val, pipeInput.SupplementVal));
			return true;
		}

		public override bool ContinuePipeLineEntry<TA, TB>(
			IValueAndSupplement<TA, TB> pipeInput,
			out PipeBase<TA, TB> wrapInputIntoPipeBase,
			out IPipeOption pipeOption)
		{
			if (pipeInput == null)
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBase = new PipeValidate.Validator<TA, TB>(pipeOption, PipeValidate.ValidationResult.None, null);
				return false;
			}

			if (pipeInput.GetType() != typeof(Validator<TA, TB>))
			{
				pipeOption = PipeOption.PipeOptionNone;
				wrapInputIntoPipeBase = new Validator<TA, TB>(new InvalidCastException(), pipeOption, null);
				return false;
			}

			var validatorType = ((Validator<TA, TB>) pipeInput).ValidatorType;
			pipeOption = (IPipeOption) pipeInput;


			if (validatorType == ValidationResult.None)
			{
				wrapInputIntoPipeBase = new Validator<TA, TB>(pipeOption, ValidationResult.None, null);
				return false;
			}

			if (validatorType == ValidationResult.Exception)
			{
				wrapInputIntoPipeBase = new Validator<TA, TB>(((Validator<TA, TB>) pipeInput).ExceptionVal, pipeOption, null);
				return false;
			}

			var msgs = ExtractMessages(pipeInput.Val, pipeInput.SupplementVal);

			wrapInputIntoPipeBase = new Validator<TA, TB>(new ValueAndSupplement<TA, TB>(pipeInput.Val, pipeInput.SupplementVal), pipeOption, msgs);
			return true;
		}

		internal Validator(IPipeOption pipeOption, ValidationResult type, List<string> messages)
		{
			ConditionMet = pipeOption.ConditionIsMet;
			ExtensionInterfaces = pipeOption.Extensions;
			ValidatorType = type;
		}

		internal Validator(Exception ex, IPipeOption pipeOption, List<string> messages)
		{
			ConditionMet = pipeOption.ConditionIsMet;
			ExtensionInterfaces = pipeOption.Extensions;
			ValidatorType = ValidationResult.Exception;
			this.ex = ex;
		}

		public Validator()
		{
			ExtensionInterfaces = null;
			ValidatorType = ValidationResult.Some;
		}

		internal Validator(IValueAndSupplement<TV, TS> val, IPipeOption pipeOption, List<string> messages)
		{
			ConditionMet = pipeOption.ConditionIsMet;
			ExtensionInterfaces = pipeOption.Extensions;
			ValidatorType = val == null ? ValidationResult.None : ValidationResult.Some;
			var extractedMessages = messages ?? new List<string>();
			if (extractedMessages.Count > 0)
				this.ValidatorType = ValidationResult.SomeInvalid;

			if (val != null)
			{
				GetValue = val.Val;
				GetSupplement = val.SupplementVal;
			}
		}

		public override TS SupplementVal => (TS) this.GetSupplement;

		public override TV Val => (TV) this.GetValue;

		public Exception ExceptionVal
		{
			get
			{
				if (ValidatorType == ValidationResult.Exception)
					return this.ex ?? new InvalidOperationException();
				throw new InvalidOperationException();
			}
		}
	}
}