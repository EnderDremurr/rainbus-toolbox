using System;
using Avalonia;

namespace RainbusToolbox.Utilities;

public static class ExceptionHelper
{
    /// <summary>
    /// Handles an unhandled exception by showing the global error dialog and shutting down the app
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    public static void HandleGlobalException(Exception exception)
    {
        try
        {
            if (Application.Current is App app)
            {
                app.HandleGlobalException(exception);
            }
            else
            {
                // Fallback: just throw to let the AppDomain handler catch it
                throw exception;
            }
        }
        catch
        {
            // Last resort: exit the application
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Wraps an action in a try-catch block and handles any exceptions globally
    /// </summary>
    /// <param name="action">The action to execute safely</param>
    public static void ExecuteSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            HandleGlobalException(ex);
        }
    }

    /// <summary>
    /// Wraps a function in a try-catch block and handles any exceptions globally
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="func">The function to execute safely</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <returns>The result of the function or the default value</returns>
    public static T ExecuteSafely<T>(Func<T> func, T defaultValue = default)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            HandleGlobalException(ex);
            return defaultValue;
        }
    }

    /// <summary>
    /// Wraps an async function in a try-catch block and handles any exceptions globally
    /// </summary>
    /// <param name="asyncFunc">The async function to execute safely</param>
    public static async System.Threading.Tasks.Task ExecuteSafelyAsync(Func<System.Threading.Tasks.Task> asyncFunc)
    {
        try
        {
            await asyncFunc();
        }
        catch (Exception ex)
        {
            HandleGlobalException(ex);
        }
    }

    /// <summary>
    /// Wraps an async function in a try-catch block and handles any exceptions globally
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="asyncFunc">The async function to execute safely</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <returns>The result of the function or the default value</returns>
    public static async System.Threading.Tasks.Task<T> ExecuteSafelyAsync<T>(Func<System.Threading.Tasks.Task<T>> asyncFunc, T defaultValue = default)
    {
        try
        {
            return await asyncFunc();
        }
        catch (Exception ex)
        {
            HandleGlobalException(ex);
            return defaultValue;
        }
    }
}