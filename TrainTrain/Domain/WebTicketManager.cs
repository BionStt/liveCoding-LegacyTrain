﻿using System.Threading.Tasks;

namespace TrainTrain.Domain
{
    // Note: now it has nothing to deal with its Web API, we should 
    // have rename it to SeatReservationService.
    public class WebTicketManager : IReserveSeats
    {
        private readonly IBookingReferenceService _bookingReferenceService;

        private readonly ITrainDataService _trainDataService;

        public WebTicketManager(ITrainDataService trainDataService, IBookingReferenceService bookingReferenceService)
        {
            _trainDataService = trainDataService;
            _bookingReferenceService = bookingReferenceService;
        }

        public async Task<Reservation> Reserve(string trainId, int seatsRequestedCount)
        {
            var train = await _trainDataService.GetTrain(trainId);

            if (train.DoesNotExceedOveralTrainCapacity(seatsRequestedCount))
            {
                var reservationAttempt = train.BuildReservationAttempt(trainId, seatsRequestedCount);

                if (reservationAttempt.IsFulfilled)
                {
                    var bookingRef = await _bookingReferenceService.GetBookingReference();

                    reservationAttempt.AssignBookingReference(bookingRef);

                    await _trainDataService.BookSeats(trainId, bookingRef, reservationAttempt.Seats);

                    var reservation = reservationAttempt.Confirm();

                    return reservation;
                }
            }
            return new FailedReservation(trainId);
        }
    }
}