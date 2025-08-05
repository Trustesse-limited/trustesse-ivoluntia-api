using System;
using Microsoft.AspNetCore.Http;

namespace Trustesse.Ivoluntia.Commons.DTOs;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }


    public static ApiResponse<T> Failure(int statusCode, string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message
        };
    }

    public static ApiResponse<T> Success(string message, T data)
    {
        return new ApiResponse<T>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            Data = data
        };
    }
}
