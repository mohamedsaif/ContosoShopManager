using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Cognitive
{
    //Generated using https://app.quicktype.io/
    public class FaceAuth : CognitiveBase
    {
        [JsonProperty("faceId")]
        public string FaceId { get; set; }

        [JsonProperty("faceRectangle")]
        public FaceRectangle FaceRectangle { get; set; }

        [JsonProperty("faceAttributes")]
        public FaceAttributes FaceAttributes { get; set; }

        [JsonProperty("isIdentical")]
        public bool IsIdentical { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class FaceAttributes
    {
        [JsonProperty("smile")]
        public double Smile { get; set; }

        [JsonProperty("headPose")]
        public HeadPose HeadPose { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("facialHair")]
        public FacialHair FacialHair { get; set; }

        [JsonProperty("glasses")]
        public string Glasses { get; set; }

        [JsonProperty("emotion")]
        public Emotion Emotion { get; set; }

        [JsonProperty("blur")]
        public Blur Blur { get; set; }

        [JsonProperty("exposure")]
        public Exposure Exposure { get; set; }

        [JsonProperty("noise")]
        public Noise Noise { get; set; }

        [JsonProperty("makeup")]
        public Makeup Makeup { get; set; }

        [JsonProperty("accessories")]
        public Accessory[] Accessories { get; set; }

        [JsonProperty("occlusion")]
        public Occlusion Occlusion { get; set; }

        [JsonProperty("hair")]
        public Hair Hair { get; set; }
    }

    public class Accessory
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class Blur
    {
        [JsonProperty("blurLevel")]
        public Level BlurLevel { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class Emotion
    {
        [JsonProperty("anger")]
        public long Anger { get; set; }

        [JsonProperty("contempt")]
        public double Contempt { get; set; }

        [JsonProperty("disgust")]
        public long Disgust { get; set; }

        [JsonProperty("fear")]
        public long Fear { get; set; }

        [JsonProperty("happiness")]
        public double Happiness { get; set; }

        [JsonProperty("neutral")]
        public double Neutral { get; set; }

        [JsonProperty("sadness")]
        public long Sadness { get; set; }

        [JsonProperty("surprise")]
        public long Surprise { get; set; }
    }

    public class Exposure
    {
        [JsonProperty("exposureLevel")]
        public string ExposureLevel { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class FacialHair
    {
        [JsonProperty("moustache")]
        public double Moustache { get; set; }

        [JsonProperty("beard")]
        public double Beard { get; set; }

        [JsonProperty("sideburns")]
        public double Sideburns { get; set; }
    }

    public class Hair
    {
        [JsonProperty("bald")]
        public double Bald { get; set; }

        [JsonProperty("invisible")]
        public bool Invisible { get; set; }

        [JsonProperty("hairColor")]
        public HairColor[] HairColor { get; set; }
    }

    public class HairColor
    {
        [JsonProperty("color")]
        public Color Color { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class HeadPose
    {
        [JsonProperty("pitch")]
        public long Pitch { get; set; }

        [JsonProperty("roll")]
        public double Roll { get; set; }

        [JsonProperty("yaw")]
        public double Yaw { get; set; }
    }

    public class Makeup
    {
        [JsonProperty("eyeMakeup")]
        public bool EyeMakeup { get; set; }

        [JsonProperty("lipMakeup")]
        public bool LipMakeup { get; set; }
    }

    public class Noise
    {
        [JsonProperty("noiseLevel")]
        public Level NoiseLevel { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class Occlusion
    {
        [JsonProperty("foreheadOccluded")]
        public bool ForeheadOccluded { get; set; }

        [JsonProperty("eyeOccluded")]
        public bool EyeOccluded { get; set; }

        [JsonProperty("mouthOccluded")]
        public bool MouthOccluded { get; set; }
    }

    public class FaceRectangle
    {
        [JsonProperty("top")]
        public long Top { get; set; }

        [JsonProperty("left")]
        public long Left { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public enum Level { High, Low, Medium };

    public enum ExposureLevel { GoodExposure };

    public enum Gender { Female, Male };

    public enum Glasses { NoGlasses, ReadingGlasses, SunGlasses };

    public enum Color { Black, Blond, Brown, Gray, Other, Red };

    public class Person
    {
        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("persistedFaceIds")]
        public string[] PersistedFaceIds { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("userData")]
        public object UserData { get; set; }
    }

    public class FaceVerifyInput
    {
        [JsonProperty("faceId")]
        public string FaceId { get; set; }

        [JsonProperty("personGroupId")]
        public string PersonGroupId { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }
    }

    public class FaceVerificationResult
    {
        [JsonProperty("isIdentical")]
        public bool IsIdentical { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
