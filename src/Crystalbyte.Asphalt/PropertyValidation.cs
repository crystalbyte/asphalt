#region Using directives

using System;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt {
    /// <summary>
    ///   Add validation support to properties
    /// </summary>
    /// <typeparam name="TBindingModel"> Model to bind </typeparam>
    public class PropertyValidation<TBindingModel> where TBindingModel : BindingModelBase<TBindingModel> {
        private Func<TBindingModel, bool> _validationCriteria;
        private string _errorMessage;

        public PropertyValidation(string propertyName) {
            PropertyName = propertyName;
        }

        public PropertyValidation<TBindingModel> When(Func<TBindingModel, bool> validationCriteria) {
            if (_validationCriteria != null)
                throw new InvalidOperationException("You can only set the validation criteria once.");
            _validationCriteria = validationCriteria;
            return this;
        }

        public PropertyValidation<TBindingModel> Show(string errorMessage) {
            if (_errorMessage != null)
                throw new InvalidOperationException("You can only set the message once.");
            _errorMessage = errorMessage;
            return this;
        }

        public bool IsInvalid(TBindingModel presentationModel) {
            if (_validationCriteria == null)
                throw new InvalidOperationException(
                    "No criteria have been provided for this validation. (Use the 'When(..)' method.)");
            return _validationCriteria(presentationModel);
        }

        public string ErrorMessage {
            get {
                if (_errorMessage == null)
                    throw new InvalidOperationException(
                        "No error message has been set for this validation. (Use the 'Show(..)' method.)");
                return _errorMessage;
            }
            set { _errorMessage = value; }
        }

        public string PropertyName { get; set; }
    }
}