﻿using Newtonsoft.Json;

namespace AzDOUpdateChildField.Classes
{
    public class Fields
    {
        public SystemRev SystemRev { get; set; }
        public SystemAuthorizeddate SystemAuthorizedDate { get; set; }
        public SystemReviseddate SystemRevisedDate { get; set; }
        public SystemState SystemState { get; set; }
        public SystemReason SystemReason { get; set; }
        public SystemAssignedto SystemAssignedTo { get; set; }
        public SystemChangeddate SystemChangedDate { get; set; }
        public SystemWatermark SystemWatermark { get; set; }
        public MicrosoftVSTSCommonSeverity MicrosoftVSTSCommonSeverity { get; set; }

        [JsonProperty("Custom.Sciforma")]
        public CustomCSPMResource CustomSciforma { get; set; }
        [JsonProperty("Custom.StudyName")]
        public CustomCSLResource CustomStudyName { get; set; }
    }

}
