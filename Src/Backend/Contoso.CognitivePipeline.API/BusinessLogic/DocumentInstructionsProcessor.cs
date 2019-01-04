using Contoso.CognitivePipeline.SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.BusinessLogic
{
    public class DocumentInstructionsProcessor
    {
        public static List<string> GetInstructions(ClassificationType docType)
        {
            List<string> instructions = new List<string>();

            switch (docType)
            {
                //All OCR cases. Note that system currently does not support of these types of documents.
                case ClassificationType.ID: //For Contoso Employee ID
                case ClassificationType.Passport:
                case ClassificationType.DriverLicense:
                    instructions.Add(InstructionFlag.AnalyzeText.ToString());
                    //TODO: Testing adding Face Authentication to be part of analyzing ID documents
                    //instructions.Add(InstructionFlag.FaceAuthentication.ToString());
                    break;
                case ClassificationType.Face: //Used to authenticate face in application sign in
                    instructions.Add(InstructionFlag.FaceAuthentication.ToString());
                    break;
                case ClassificationType.StoreShelf:
                    instructions.Add(InstructionFlag.ShelfCompliance.ToString());
                    break;
                case ClassificationType.Generic: //anything else, use Analyze Image service to describe the image
                    instructions.Add(InstructionFlag.AnalyzeImage.ToString());
                    break;
                case ClassificationType.Unidentified:
                    instructions.Add(InstructionFlag.AnalyzeImage.ToString());
                    break;
                default:
                    instructions.Add(InstructionFlag.AnalyzeImage.ToString());
                    break;
            }

            return instructions;
        }
    }
}
