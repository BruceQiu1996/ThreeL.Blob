using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.Blob.Shared.Application.Contract.Helpers;

namespace ThreeL.Blob.Shared.Application.Register
{
    public partial class ApplicationDependencyRegistrar
    {
        internal void AddHelpers()
        {
            _services.AddSingleton<PasswordHelper>();
        }

        internal void AddFluentValidator()
        {
            //_services.AddFluentValidationAutoValidation();
            _services.AddValidatorsFromAssembly(_applicationAssemblyInfo.ContractAssembly);
        }

        internal void AddAutoMapper() 
        {
            _services.AddAutoMapper(_applicationAssemblyInfo.ContractAssembly.DefinedTypes
                .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.AsType())).Select(t => t.AsType()).ToArray());
        }
    }
}
