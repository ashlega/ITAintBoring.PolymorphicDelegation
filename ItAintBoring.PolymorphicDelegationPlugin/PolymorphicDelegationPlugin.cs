using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ItAintBoring.PolymorphicDelegation
{
    
    public class PolymorphicDelegationPlugin : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            if (context.MessageName == "RetrieveMultiple")
            {
                if (context.Stage == 20)
                {
                    if (context.InputParameters.Contains("Query"))
                    {

                        QueryExpression query = null;
                        if (context.InputParameters["Query"] is FetchExpression)
                        {
                            var fe = (FetchExpression)context.InputParameters["Query"];
                            var conversionRequest = new FetchXmlToQueryExpressionRequest
                            {
                                 FetchXml = fe.Query
                            };

                            var conversionResponse =
                                (FetchXmlToQueryExpressionResponse)service.Execute(conversionRequest);
                            query = conversionResponse.Query;
                        }

                        if (context.InputParameters["Query"] is QueryExpression)
                        {
                            query = (QueryExpression)context.InputParameters["Query"];
                        }
                        ConditionExpression dummyCondition = null;
                        foreach(var c in query.Criteria.Conditions)
                        {
                            if(c.AttributeName == "ita_dummyaccountname")
                            {
                                dummyCondition = c;
                                break;
                            }
                        }
                        if(dummyCondition != null)
                        {
                            query.Criteria.Conditions.Remove(dummyCondition);
                            LinkEntity le = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner);
                            le.LinkCriteria.AddCondition("name", ConditionOperator.Equal, dummyCondition.Values.First());
                            query.LinkEntities.Add(le);
                            context.InputParameters["Query"] = query;
                        }
                        
                        
                    }
                }

            }
        }
    }
}