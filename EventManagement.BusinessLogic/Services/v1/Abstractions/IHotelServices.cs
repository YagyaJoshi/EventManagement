using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IHotelServices
    {
        Task<long> AddHotel(long? id, long organizationId, Hotel input);

        Task<HotelListDto> GetAllHotels(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize);
        Task<HotelDto> GetHotelById(long id);
        Task<long> DeleteHotel(long id);
        Task<List<HotelRoomType>> GetHotelRoomTypes(long hotelId);
        Task<int?> CheckRoomAvailability(RoomAvailabilityInput input);
        Task<List<HotelInfoDto>> GetHotelsByOrganizationId(long organizationId);
    }
}
