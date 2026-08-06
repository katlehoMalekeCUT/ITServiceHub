using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services.Ai;

public class TicketPredictionModel
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;

    public TicketPredictionModel()
    {
        _mlContext = new MLContext(seed: 1);
        _model = TrainModel();
    }

    public TicketPrediction Predict(string title, string description, string category, string priority)
    {
        var input = new TicketInput
        {
            Title = title,
            Description = description,
            Category = category,
            CurrentPriority = priority
        };

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TicketInput, TicketPredictionOutput>(_model);
        var prediction = predictionEngine.Predict(input);

        return new TicketPrediction
        {
            Priority = MapPriority(prediction.PredictedPriority),
            EstimatedResolutionHours = Math.Max(1, (int)Math.Round(prediction.EstimatedHours)),
            RecommendedTechnician = MapTechnician(prediction.PredictedTechnician)
        };
    }

    private ITransformer TrainModel()
    {
        var samples = new List<TicketTrainingSample>
        {
            new() { Title = "Laptop not charging", Description = "Battery issue", Category = "Hardware", CurrentPriority = "High", LabelPriority = "High", LabelHours = 6, LabelTechnician = "Technician A" },
            new() { Title = "Email login issue", Description = "Cannot sign in", Category = "Software", CurrentPriority = "Medium", LabelPriority = "Medium", LabelHours = 4, LabelTechnician = "Technician B" },
            new() { Title = "Printer offline", Description = "Network issue", Category = "Printer", CurrentPriority = "Critical", LabelPriority = "Critical", LabelHours = 12, LabelTechnician = "Technician C" },
            new() { Title = "VPN disconnect", Description = "Remote access issue", Category = "Network", CurrentPriority = "High", LabelPriority = "High", LabelHours = 8, LabelTechnician = "Technician D" },
            new() { Title = "Monitor not detected", Description = "Display driver issue", Category = "Hardware", CurrentPriority = "Low", LabelPriority = "Low", LabelHours = 2, LabelTechnician = "Technician A" }
        };

        var data = _mlContext.Data.LoadFromEnumerable(samples);

        var titleFeatures = _mlContext.Transforms.Text.FeaturizeText("TitleFeatures", nameof(TicketTrainingSample.Title));
        var descriptionFeatures = _mlContext.Transforms.Text.FeaturizeText("DescriptionFeatures", nameof(TicketTrainingSample.Description));
        var categoryFeatures = _mlContext.Transforms.Text.FeaturizeText("CategoryFeatures", nameof(TicketTrainingSample.Category));
        var priorityFeatures = _mlContext.Transforms.Text.FeaturizeText("PriorityFeatures", nameof(TicketTrainingSample.CurrentPriority));

        var pipeline = titleFeatures
            .Append(descriptionFeatures)
            .Append(categoryFeatures)
            .Append(priorityFeatures)
            .Append(_mlContext.Transforms.Concatenate("Features", "TitleFeatures", "DescriptionFeatures", "CategoryFeatures", "PriorityFeatures"))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: nameof(TicketTrainingSample.LabelPriority), inputColumnName: nameof(TicketTrainingSample.LabelPriority)))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: nameof(TicketTrainingSample.LabelTechnician), inputColumnName: nameof(TicketTrainingSample.LabelTechnician)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(nameof(TicketTrainingSample.LabelPriority), "Features"))
            .Append(_mlContext.Regression.Trainers.Sdca(nameof(TicketTrainingSample.LabelHours), "Features"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(nameof(TicketTrainingSample.LabelTechnician), "Features"));

        return pipeline.Fit(data);
    }

    private static string MapPriority(float value)
    {
        return value switch
        {
            >= 3 => "Critical",
            >= 2 => "High",
            >= 1 => "Medium",
            _ => "Low"
        };
    }

    private static string MapTechnician(float value)
    {
        return value switch
        {
            >= 3 => "Technician D",
            >= 2 => "Technician C",
            >= 1 => "Technician B",
            _ => "Technician A"
        };
    }

    private class TicketTrainingSample
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CurrentPriority { get; set; } = string.Empty;
        public string LabelPriority { get; set; } = string.Empty;
        public float LabelHours { get; set; }
        public string LabelTechnician { get; set; } = string.Empty;
    }

    private class TicketInput
    {
        [LoadColumn(0)] public string Title { get; set; } = string.Empty;
        [LoadColumn(1)] public string Description { get; set; } = string.Empty;
        [LoadColumn(2)] public string Category { get; set; } = string.Empty;
        [LoadColumn(3)] public string CurrentPriority { get; set; } = string.Empty;
    }

    private class TicketPredictionOutput
    {
        [ColumnName("PredictedLabel")]
        public float PredictedPriority { get; set; }

        [ColumnName("Score")]
        public float EstimatedHours { get; set; }

        [ColumnName("PredictedLabel")]
        public float PredictedTechnician { get; set; }
    }
}
