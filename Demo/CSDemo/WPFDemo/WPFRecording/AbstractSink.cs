namespace WPFRecording
{
   internal abstract class AbstractSink
   {
      #region Public methods

      public abstract object getOutputNode(object aUpStreamMediaType);
      public abstract void setOptions(string aOptions);

      #endregion
   }
}
