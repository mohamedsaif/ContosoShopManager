using Contoso.CognitivePipeline.SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.API.Helpers
{
    public static class OutputOptimizationExtensions
    {
        public static void OptimizeSmartDoc(this SmartDoc doc, bool isMinimum)
        {
            if (!isMinimum)
                return;

            doc.CognitivePipelineActions = null;
            doc.DocName = "";
            doc.DocUrl = "";
            doc.IconSizeUrl = "";
            doc.TileSizeUrl = "";

            //Type specific optimization:
            if(doc is ShelfCompliance)
            {
                var temp = doc as ShelfCompliance;
                //Remove all matched classification result.
                temp.Classification = null;
            }
            else if(doc is EmployeeId)
            {
                var temp = doc as EmployeeId;
                //No further optmization needed for EmployeeId type
            }
            else if (doc is FaceAuthCard)
            {
                var temp = doc as FaceAuthCard;
                //Remove all face details. Just final authentication results will be part of the response.
                temp.FaceDetails = null;
            }
        }
    }
}
