using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;

namespace MediatR.Examples.StructureMap.WebAPI.Areas.HelpPage
{
    /// <summary>
    /// This is used to identify the place where the sample should be applied.
    /// </summary>
    public class HelpPageSampleKey
    {
        /// <summary>
        /// Creates a new <see cref="HelpPageSampleKey"/> based on media type.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        public HelpPageSampleKey(MediaTypeHeaderValue mediaType)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            ActionName = String.Empty;
            ControllerName = String.Empty;
            MediaType = mediaType;
            ParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a new <see cref="HelpPageSampleKey"/> based on media type and CLR type.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="type">The CLR type.</param>
        public HelpPageSampleKey(MediaTypeHeaderValue mediaType, Type type)
            : this(mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ParameterType = type;
        }

        /// <summary>
        /// Creates a new <see cref="HelpPageSampleKey"/> based on <see cref="SampleDirection"/>, controller name, action name and parameter names.
        /// </summary>
        /// <param name="sampleDirection">The <see cref="SampleDirection"/>.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public HelpPageSampleKey(SampleDirection sampleDirection, string controllerName, string actionName, IEnumerable<string> parameterNames)
        {
            if (!Enum.IsDefined(typeof(SampleDirection), sampleDirection))
            {
                throw new InvalidEnumArgumentException("sampleDirection", (int)sampleDirection, typeof(SampleDirection));
            }
            if (controllerName == null)
            {
                throw new ArgumentNullException("controllerName");
            }
            if (actionName == null)
            {
                throw new ArgumentNullException("actionName");
            }
            if (parameterNames == null)
            {
                throw new ArgumentNullException("parameterNames");
            }

            ControllerName = controllerName;
            ActionName = actionName;
            ParameterNames = new HashSet<string>(parameterNames, StringComparer.OrdinalIgnoreCase);
            SampleDirection = sampleDirection;
        }

        /// <summary>
        /// Creates a new <see cref="HelpPageSampleKey"/> based on media type, <see cref="SampleDirection"/>, controller name, action name and parameter names.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="sampleDirection">The <see cref="SampleDirection"/>.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public HelpPageSampleKey(MediaTypeHeaderValue mediaType, SampleDirection sampleDirection, string controllerName, string actionName, IEnumerable<string> parameterNames)
            : this(sampleDirection, controllerName, actionName, parameterNames)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            MediaType = mediaType;
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        /// <value>
        /// The name of the controller.
        /// </value>
        public string ControllerName { get; private set; }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <value>
        /// The name of the action.
        /// </value>
        public string ActionName { get; private set; }

        /// <summary>
        /// Gets the media type.
        /// </summary>
        /// <value>
        /// The media type.
        /// </value>
        public MediaTypeHeaderValue MediaType { get; private set; }

        /// <summary>
        /// Gets the parameter names.
        /// </summary>
        public HashSet<string> ParameterNames { get; private set; }

        public Type ParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="SampleDirection"/>.
        /// </summary>
        public SampleDirection? SampleDirection { get; private set; }

        public override bool Equals(object obj)
        {
            HelpPageSampleKey otherKey = obj as HelpPageSampleKey;
            if (otherKey == null)
            {
                return false;
            }

            return String.Equals(ControllerName, otherKey.ControllerName, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(ActionName, otherKey.ActionName, StringComparison.OrdinalIgnoreCase) &&
                (MediaType == otherKey.MediaType || (MediaType != null && MediaType.Equals(otherKey.MediaType))) &&
                ParameterType == otherKey.ParameterType &&
                SampleDirection == otherKey.SampleDirection &&
                ParameterNames.SetEquals(otherKey.ParameterNames);
        }

        public override int GetHashCode()
        {
            int hashCode = ControllerName.ToUpperInvariant().GetHashCode() ^ ActionName.ToUpperInvariant().GetHashCode();
            if (MediaType != null)
            {
                hashCode ^= MediaType.GetHashCode();
            }
            if (SampleDirection != null)
            {
                hashCode ^= SampleDirection.GetHashCode();
            }
            if (ParameterType != null)
            {
                hashCode ^= ParameterType.GetHashCode();
            }
            foreach (string parameterName in ParameterNames)
            {
                hashCode ^= parameterName.ToUpperInvariant().GetHashCode();
            }

            return hashCode;
        }
    }
}
