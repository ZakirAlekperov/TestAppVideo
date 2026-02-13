namespace TestAppVideo.Domain.VideoProcessing.Ports;

public interface IVideoProcessingProgress
{
    void ReportProgress(int currentSegment, int totalSegments, double percentComplete);
    void ReportSegmentCompleted(int segmentNumber, string outputFilePath);
    void ReportSegmentError(int segmentNumber, Exception error);
}
