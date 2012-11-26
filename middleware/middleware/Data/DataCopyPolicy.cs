public enum DataCopyPolicy
{
    /// <summary>
    /// If no copying of data before further processing is necessary<br />
    /// I.e. data is not further modified by the publisher and can be modified in-place by the subscriber
    /// </summary>
    None,
    /// <summary>
    /// Data should be copied before processing <br />
    /// I.e. the publisher will reuse the data contained in further processing steps
    /// </summary>
    Copy
}