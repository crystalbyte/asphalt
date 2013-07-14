﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Contexts {
    /// <summary>
    /// BindingModel to support validation and handling of error messages (Implements INotifyDataError)
    /// </summary>
    /// <typeparam name="TBindingModel">Model to bind</typeparam>
    public abstract class BindingModelBase<TBindingModel> : RevisionObject, INotifyDataErrorInfo
        where TBindingModel : BindingModelBase<TBindingModel> {

        private readonly List<PropertyValidation<TBindingModel>> _validations =
            new List<PropertyValidation<TBindingModel>>();

        private Dictionary<string, List<string>> _errorMessages = new Dictionary<string, List<string>>();

        protected BindingModelBase() {
            PropertyChanged +=
                (s, e) => {
                    if (e.PropertyName != "HasErrors" && e.PropertyName != "ErrorMessages")
                        ValidateProperty(e.PropertyName);
                };
        }

        #region INotifyDataErrorInfo

        public IEnumerable GetErrors(string propertyName) {
            if (_errorMessages.ContainsKey(propertyName))
                return _errorMessages[propertyName];
            return new string[0];
        }

        public bool HasErrors {
            get { return _errorMessages.Any(); }
        }

        public Dictionary<string, List<string>> ErrorMessages {
            get { return _errorMessages; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnErrorsChanged(string propertyName) {
            var handler = ErrorsChanged;
            if (handler != null) {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #endregion

        protected PropertyValidation<TBindingModel> AddValidationFor(Expression<Func<object>> expression) {
            return AddValidationFor(GetPropertyName(expression));
        }

        protected PropertyValidation<TBindingModel> AddValidationFor(string propertyName) {
            var validation = new PropertyValidation<TBindingModel>(propertyName);
            _validations.Add(validation);
            return validation;
        }

        public void ValidateAll() {
            var propertyNamesWithValidationErrors = _errorMessages.Keys;
            _errorMessages = new Dictionary<string, List<string>>();
            _validations.ForEach(PerformValidation);

            var propertyNamesThatMightHaveChangedValidation =
                _errorMessages.Keys.Union(propertyNamesWithValidationErrors).ToList();
            propertyNamesThatMightHaveChangedValidation.ForEach(OnErrorsChanged);

            RaisePropertyChanged(() => HasErrors);
            RaisePropertyChanged(() => ErrorMessages);
        }

        public void ValidateProperty(Expression<Func<object>> expression) {
            ValidateProperty(GetPropertyName(expression));
        }

        private void ValidateProperty(string propertyName) {
            _errorMessages.Remove(propertyName);
            _validations.Where(v => v.PropertyName == propertyName).ToList().ForEach(PerformValidation);
            OnErrorsChanged(propertyName);

            RaisePropertyChanged(() => HasErrors);
            RaisePropertyChanged(() => ErrorMessages);
        }

        protected void ValidateValue(Func<bool> function, string message) {
            var success = function();
            if (!success) {
                throw new ValidationException(message);
            }
        }

        private void PerformValidation(PropertyValidation<TBindingModel> validation) {
            if (validation.IsInvalid((TBindingModel)this)) {
                AddErrorMessageForProperty(validation.PropertyName, validation.GetErrorMessage());
            }
        }

        private void AddErrorMessageForProperty(string propertyName, string errorMessage) {
            if (_errorMessages.ContainsKey(propertyName)) {
                _errorMessages[propertyName].Add(errorMessage);
            } else {
                _errorMessages.Add(propertyName, new List<string> { errorMessage });
            }
        }

        private static string GetPropertyName(Expression<Func<object>> expression) {
            if (expression == null)
                throw new ArgumentNullException("expression");
            MemberExpression memberExpression;
            if (expression.Body is UnaryExpression)
                memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException(AppResources.ExpressionNoMember, "expression");
            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException(AppResources.MemberExpressionNoProperty, "expression");
            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
                throw new ArgumentException(AppResources.ReferencedPropertyIsStatic, "expression");
            return memberExpression.Member.Name;
        }
    }
}