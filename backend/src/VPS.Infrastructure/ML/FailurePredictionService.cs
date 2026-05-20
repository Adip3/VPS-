using Microsoft.ML;
using Microsoft.ML.Data;
using VPS.Application.DTOs;
using VPS.Application.Interfaces;

namespace VPS.Infrastructure.ML;

public class FailureInput
{
    public string PartType { get; set; } = "";
    public float MileageKm { get; set; }
    public float MonthsInstalled { get; set; }
    public float AvgLifespanKm { get; set; }
    public float Label { get; set; }  // failure prob 0-1 (training)
}

public class FailureOutput
{
    [ColumnName("Score")] public float Score { get; set; }
}

public class FailurePredictionService : IFailurePredictionService
{
    private readonly PredictionEngine<FailureInput, FailureOutput> _engine;

    public FailurePredictionService()
    {
        var ml = new MLContext(seed: 1);

        // Synthetic training data — heuristic: prob ~ usedFraction = miles / lifespan + age factor
        var rows = new List<FailureInput>();
        var rnd = new Random(1);
        string[] parts = { "Brake", "Engine", "Filter", "Battery", "Tyre" };
        for (int i = 0; i < 500; i++)
        {
            var pt = parts[rnd.Next(parts.Length)];
            var life = rnd.Next(20000, 80000);
            var used = rnd.Next(0, life + 30000);
            var months = rnd.Next(1, 60);
            var prob = Math.Min(1f, (float)used / life * 0.7f + months / 60f * 0.3f);
            prob = Math.Clamp(prob + (float)(rnd.NextDouble() - 0.5) * 0.1f, 0, 1);
            rows.Add(new FailureInput { PartType = pt, MileageKm = used, MonthsInstalled = months, AvgLifespanKm = life, Label = prob });
        }

        var data = ml.Data.LoadFromEnumerable(rows);
        var pipeline = ml.Transforms.Categorical.OneHotEncoding("PartTypeEnc", "PartType")
            .Append(ml.Transforms.Concatenate("Features", "PartTypeEnc", "MileageKm", "MonthsInstalled", "AvgLifespanKm"))
            .Append(ml.Transforms.NormalizeMinMax("Features"))
            .Append(ml.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

        var model = pipeline.Fit(data);
        _engine = ml.Model.CreatePredictionEngine<FailureInput, FailureOutput>(model);
    }

    public FailurePredictResultDto Predict(FailurePredictDto dto)
    {
        var input = new FailureInput
        {
            PartType = dto.PartType,
            MileageKm = dto.MileageKm,
            MonthsInstalled = dto.MonthsInstalled,
            AvgLifespanKm = dto.AvgLifespanKm
        };
        var r = _engine.Predict(input);
        var prob = Math.Clamp(r.Score, 0, 1);
        var rec = prob switch
        {
            > 0.75f => "Replace soon — high risk",
            > 0.5f => "Inspect — moderate wear",
            > 0.25f => "Monitor — light wear",
            _ => "Healthy"
        };
        return new FailurePredictResultDto(prob, rec);
    }
}
