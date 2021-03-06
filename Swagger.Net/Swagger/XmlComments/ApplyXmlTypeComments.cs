﻿using System.Reflection;
using System.Xml.XPath;

namespace Swagger.Net.XmlComments
{
    public class ApplyXmlTypeComments : IModelFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string ExampleTag = "example";

        private readonly XPathDocument _xmlDoc;

        public ApplyXmlTypeComments(string filePath)
            : this(new XPathDocument(filePath)) { }

        public ApplyXmlTypeComments(XPathDocument xmlDoc)
        {
            _xmlDoc = xmlDoc;
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            XPathNavigator navigator;
            lock (_xmlDoc)
            {
                navigator = _xmlDoc.CreateNavigator();
            }

            var commentId = context.SystemType.GetCommentIdForType();
            var typeNode = navigator.SelectSingleNode(string.Format(MemberXPath, commentId));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode( SummaryTag );
                if( summaryNode != null )
                    model.description = summaryNode.ExtractContent();

                var exampleNode = typeNode.SelectSingleNode( ExampleTag );
                if( exampleNode != null )
                    model.example = exampleNode.ExtractContent();
            }

            if (model.properties != null)
            {
                foreach (var entry in model.properties)
                {
                    var jsonProperty = context.JsonObjectContract.Properties[entry.Key];
                    if (jsonProperty == null) continue;

                    ApplyPropertyComments(navigator, entry.Value, jsonProperty.PropertyInfo());
                }
            }
        }

        private static void ApplyPropertyComments(XPathNavigator navigator, Schema propertySchema, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return;

            var commentId = propertyInfo.GetCommentIdForProperty();
            var propertyNode = navigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (propertyNode == null) return;

            var propSummaryNode = propertyNode.SelectSingleNode(SummaryTag);
            if (propSummaryNode != null)
            {
                propertySchema.description = propSummaryNode.ExtractContent();
            }

            var propExampleNode = propertyNode.SelectSingleNode( ExampleTag );
            if( propExampleNode != null )
            {
                propertySchema.example = propExampleNode.ExtractContent();
            }
        }
    }
}