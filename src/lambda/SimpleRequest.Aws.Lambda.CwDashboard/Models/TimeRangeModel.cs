namespace SimpleRequest.Aws.Lambda.CwDashboard.Models;

public interface ITimeRangeModel {
    long Start { get; }
    
    long End { get; }
}

public record TimeRangeZoomModel(long Start, long End);

public record TimeRangeModel(string Mode, long Start, long End, long RelativeStart, TimeRangeZoomModel? Zoom);