using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Languages.Editor.Services {
    /// <summary>
    /// Assist in location of services specific to a particular file content type.
    /// </summary>
    public interface IContentTypeServiceLocator {
        /// <summary>
        /// Locates services for a content type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Service instance, if any</returns>
        T GetService<T>(string contentType) where T : class;

        /// <summary>
        /// Retrieves all services of a particular type available for the content type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Collection of service instances, if any</returns>
        IEnumerable<T> GetAllServices<T>(string contentType) where T : class;

        /// <summary>
        /// Retrieves all services of a particular type available for the content type.
        /// Services are ordered in a way they should be accessed. This applies, for example,
        /// to command controller factories so controllers are called in a specific order.
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="contentType">Content (file) type such as 'R' or 'Markdown'</param>
        /// <returns>Collection of service instances, if any</returns>
        IEnumerable<Lazy<T>> GetAllOrderedServices<T>(string contentType) where T : class;
    }
}
