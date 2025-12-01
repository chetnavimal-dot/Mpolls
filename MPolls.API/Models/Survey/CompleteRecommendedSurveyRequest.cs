using System;
using System.ComponentModel.DataAnnotations;

namespace MPolls.API.Models.Survey;

public class CompleteRecommendedSurveyRequest
{
    [DataType(DataType.DateTime)]
    public DateTime? CompletedOn { get; set; }
}
