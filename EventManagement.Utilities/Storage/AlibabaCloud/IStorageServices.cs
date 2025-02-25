namespace EventManagement.Utilities.Storage.AlibabaCloud
{
    public interface IStorageServices
    {
        Task<List<string>> UploadFiles(List<string> imagePaths, string folderName, long organizationId);
        Task<string> UploadFile(string imagePath, string folderName, long organizationId, long? bookingId = null);
    }
}
