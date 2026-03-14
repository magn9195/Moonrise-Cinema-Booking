using System.Text.Json;

namespace CinemaWebService.Models.Enum.EnumHelper
{
	public static class EnumSerializer
	{
		public static string GetSeatStatusJson()
		{
			return JsonSerializer.Serialize(new
			{
				Available = (int)SeatStatusEnum.Available,
				Reserved = (int)SeatStatusEnum.Reserved,
				Booked = (int)SeatStatusEnum.Booked,
				Unavailable = (int)SeatStatusEnum.Unavailable
			});
		}

		public static string GetSeatTypeJson()
		{
			return JsonSerializer.Serialize(new
			{
				Front = (int)SeatTypeEnum.Front,
				Standard = (int)SeatTypeEnum.Standard,
				Handicapped = (int)SeatTypeEnum.Handicapped
			});
		}

		public static string GetTicketTypeJson()
		{
			return JsonSerializer.Serialize(new
			{
				Adult = (int)TicketTypeEnum.Adult,
				Child = (int)TicketTypeEnum.Child,
				Student = (int)TicketTypeEnum.Student,
				Senior = (int)TicketTypeEnum.Senior,
				VIP = (int)TicketTypeEnum.VIP
			});
		}

		public static string GetShowTypeJson()
		{
			return JsonSerializer.Serialize(new
			{
				Normal = (int)ShowTypeEnum.Normal,
				IMax = (int)ShowTypeEnum.IMax,
				BabyBio = (int)ShowTypeEnum.BabyBio,
				ThirdDimension = (int)ShowTypeEnum.ThirdDimension,
				StrikkeBio = (int)ShowTypeEnum.StrikkeBio,
				Event = (int)ShowTypeEnum.Event
			});
		}

	}
}
