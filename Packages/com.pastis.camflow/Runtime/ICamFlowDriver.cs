namespace Pastis.CamFlow
{
    /// <summary>
    /// Anything that can drive the camera should implement this.
    /// It can live outside the package too (user scripts).
    /// </summary>
    public interface ICamFlowDriver
    {
        /// <summary>
        /// Return true if you want to take control this frame and output a command.
        /// Return false to let CamFlow use its default input behavior.
        /// </summary>
        bool TryGetCommand(out CameraCommand command);
    }
}
