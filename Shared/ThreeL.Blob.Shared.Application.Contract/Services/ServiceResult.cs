﻿using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ThreeL.Blob.Shared.Application.Contract.Services
{
    public class ServiceResult<T>
    {
        public ServiceResult(T t)
        {
            Value = t;
        }

        public ServiceResult(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public ServiceResult()
        {

        }

        public T Value { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string Message { get; set; }
    }

    public class ServiceResult
    {
        public ServiceResult(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public ServiceResult()
        {

        }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string Message { get; set; }
    }
}
