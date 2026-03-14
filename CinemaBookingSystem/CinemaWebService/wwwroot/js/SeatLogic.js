let selectedSeats = [];
let selectedTicketTypes = [];

/**
 * Builds the seat grid dynamically based on API data
 * @param {Array} seatAvailability - Array of GetShowtimeSeatCS objects
 */
function buildDynamicGrid(seatAvailability) {
    const parent = document.querySelector('.seat-grid');

    if (!parent) {
        console.error('Seat grid container not found');
        return;
    }

    if (!seatAvailability || seatAvailability.length === 0) {
        parent.innerHTML = '<p>No seats available</p>';
        return;
    }

    parent.innerHTML = '';

    // Group seats by row number
    const groupedSeats = Object.groupBy(seatAvailability, item => item.seat.rowNo);
    // Sort rows by row number
    const sortedRows = Object.keys(groupedSeats).sort((a, b) => Number(a) - Number(b));

    sortedRows.forEach(rowNo => {
        const row = document.createElement('div');
        row.className = 'row';
        row.dataset.rowNo = rowNo;

        // Sort seats and create seat elements
        const seatsInRow = groupedSeats[rowNo].sort((a, b) => a.seat.seatNo - b.seat.seatNo);
        seatsInRow.forEach(seatData => {
            const spanContainer = createSeatElement(seatData, rowNo);
            row.appendChild(spanContainer);
        });

        parent.appendChild(row);
    });
}

/**
 * Creates a seat element with appropriate styling based on status and type
 * @param {Object} seatData - GetShowtimeSeatCS object
 * @param {string} rowNo - Row number
 */
function createSeatElement(seatData, rowNo) {
    // Read seat properties
    const { seat, status } = seatData;
    const { seatID, seatNo, seatType } = seat;
    const seatId = `seat-row${rowNo}-seat${seatNo}`;

    const spanContainer = document.createElement('div');
    spanContainer.className = 'spanContainer';
    spanContainer.id = `span-${seatId}`;

    const seatColor = getSeatColor(status, seatType);
    const isInteractive = status === SeatStatus.Available;

    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.setAttribute('class', 'seat-svg');
    svg.setAttribute('id', seatId);
    svg.setAttribute('viewBox', '0 0 32 26');
    svg.setAttribute('width', '32');
    svg.setAttribute('height', '26');
    svg.setAttribute('xmlns', 'http://www.w3.org/2000/svg');

    // Store seat data on the SVG element
    svg.dataset.seatId = seat.seatID;
    svg.dataset.rowNo = seat.rowNo;
    svg.dataset.seatNo = seat.seatNo;
    svg.dataset.seatType = seat.seatType;
    svg.dataset.status = status;
    svg.dataset.interactive = isInteractive;

    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    path.setAttribute('d', 'M2.7,19.2c-0.8,0-1.5-0.5-1.8-1.3c-0.7-2.2-1-4.8-1-7.8c0-3,0.2-5.2,0.5-6.8c0.2-1,1.2-1.6,2.2-1.4c1,0.2,1.6,1.2,1.4,2.2c-0.3,1.3-0.4,3.3-0.4,6c0,2.7,0.3,4.9,0.8,6.8c0.3,1-0.3,2-1.2,2.3C3.1,19.2,2.9,19.2,2.7,19.2z M31,17.9c0.7-2.2,1-4.8,1-7.8c0-3-0.2-5.2-0.5-6.8c-0.2-1-1.2-1.6-2.2-1.4c-1,0.2-1.6,1.2-1.4,2.2c0.3,1.3,0.4,3.3,0.4,6c0,2.7-0.3,4.9-0.8,6.8c-0.3,1,0.3,2,1.2,2.3c0.2,0.1,0.4,0.1,0.5,0.1C30,19.2,30.8,18.7,31,17.9z M6.4,3.5c-0.3,1.7-0.5,3.9-0.5,6.5c0,2.6,0.1,4.7,0.4,6.3c0.2,1.3,1.2,2.4,2.4,2.8c1.8,0.6,4.2,0.9,7.2,0.9s5.4-0.3,7.2-0.9c1.3-0.4,2.2-1.5,2.4-2.8c0.3-1.7,0.4-3.8,0.4-6.3c0-2.7-0.2-4.8-0.5-6.5c-0.3-1.3-1.2-2.4-2.6-2.8c-1.6-0.5-4-0.7-7-0.7s-5.4,0.2-7,0.7C7.7,1.1,6.7,2.2,6.4,3.5z M26.9,24c0.9-0.5,1.3-1.5,0.8-2.5c-0.5-0.9-1.6-1.3-2.5-0.8c-2.2,1.1-5.3,1.6-9.2,1.6c-4,0-7.1-0.6-9.2-1.6c-0.9-0.4-2-0.1-2.5,0.8c-0.5,0.9-0.1,2,0.8,2.5c2.7,1.4,6.3,2,10.9,2C20.6,26,24.2,25.3,26.9,24z');
    path.style.fill = seatColor;

    svg.appendChild(path);
    spanContainer.appendChild(svg);

    return spanContainer;
}

/**
 * Determines the color of a seat based on its status and type
 * @param {number} status - SeatStatus enum value
 * @param {number} seatType - SeatType enum value
 * @returns {string} Color value
 */
function getSeatColor(status, seatType) {
    if (status === SeatStatus.Reserved) {
        return '#FFFB00'; // Yellow - Reserved
    }

    if (status === SeatStatus.Booked) {
        return '#ff0000'; // Red - Booked
    }

    if (status === SeatStatus.Unavailable) {
        return '#808080'; // Grey - Unavailable
    }

    if (seatType === SeatType.Handicapped && status != SeatStatus.Booked && status != SeatStatus.Reserved) {
        return '#0000ff'; // Blue - Handicap
    }

    return '#000000'; // Black - Available
}

/**
 * SignalR Functionality for Seats
 */
document.addEventListener('DOMContentLoaded', function () {
    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/SeatBookingHub")
        .withAutomaticReconnect()
        .build();

    const connectionStatus = document.getElementById('connectionStatus');

    connection.start()
        .then(() => {
            console.log('SignalR Connected');
        })
        .catch(err => {
            console.error('SignalR Connection Error: ', err);
        });

    connection.onreconnecting(() => {
        console.log('Reconnecting...');
    });

    connection.onreconnected(() => {
        console.log('Reconnected');
    });

    connection.onclose(() => {
        console.log('Disconnected');
    });

    // Listen for seat booking updates from other clients
    connection.on("ReceiveSeatBooked", (showtimeID, seatID) => {
        console.log(`Received seat booking notification: ID ${seatID} In Showtime ${showtimeID})`);
        refreshSeatData(showtimeID, seatID);
    });

    // Listen for seat reservation updates from other clients
    connection.on("ReceiveSeatReserved", (showtimeID, seatID) => {
        console.log(`Seat reserved: ${seatID} in showtime ${showtimeID}`);
        refreshSeatData(showtimeID, seatID);
    });

});

/**
 * Fetches fresh seat data and updates the specific seat in the UI
 * @param {number} showtimeID - The showtime ID
 * @param {number} seatID - The seat ID that was booked
 */
async function refreshSeatData(showtimeID, seatID) {
    try {
        // Fetch updated seat availability from API via MVC Controller
        const url = `/Cinema/GetShowtimeSeats?showtimeID=${showtimeID}`;
        const response = await fetch(url);

        if (!response.ok) {
            console.error('Failed to fetch seat data. Status:', response.status, 'Error:', errorText);
            return;
        }

        const seatData = await response.json();
        const updatedSeat = seatData.find(s => s.seat.seatID === seatID);

        if (updatedSeat) {
            updateSeatInUI(updatedSeat);
        }
        else {
            console.warn('Seat not found in response data');
        }
    } catch (error) {
        console.error('Error refreshing seat data:', error);
    }
}

/**
 * Updates a single seat element in the UI
 * @param {Object} seatData - The updated seat data
 */
function updateSeatInUI(seatData) {
    const { seat, status } = seatData;

    if (!seat) {
        console.error('Seat data is undefined:', seatData);
        return;
    }

    const seatId = `seat-row${seat.rowNo}-seat${seat.seatNo}`;
    const svg = document.getElementById(seatId);

    if (!svg) {
        console.warn(`Seat element not found: ${seatId}`);
        return;
    }

    const path = svg.querySelector('path');

    svg.dataset.status = status;
    svg.dataset.interactive = (status === SeatStatus.Available).toString();

    // Remove any selection if this seat was selected
    if (path.classList.contains('clicked')) {
        path.classList.remove('clicked');
        const seatIdNum = seat.SeatID.toString();
        const seatIndex = selectedSeats.indexOf(seatIdNum);
        if (seatIndex > -1) {
            selectedSeats.splice(seatIndex, 1); // Remove seat from selectedSeats
            selectedTicketTypes.splice(seatIndex, 1); // Remove corresponding ticket type
        }
    }

    // Update the color based on new status
    const newColor = getSeatColor(status, seat.seatType);
    path.style.fill = newColor;

    console.log(`Seat ${seatId} updated to status: ${status}`);
}

/**
 * Handles seat click events
 */
const onClick = (event) => {
    let svg = null;

    // Check if clicked on valid seat element
    if (event.target.tagName === 'path') {
        svg = event.target.closest('.seat-svg');
    }

    else if (event.target.classList.contains('seat-svg')) {
        svg = event.target;
    }

    else if (event.target.classList.contains('spanContainer')) {
        svg = event.target.querySelector('.seat-svg');
    }

    // Toggle selection
    if (svg && svg.dataset.interactive === 'true') {
        const path = svg.querySelector('path');
        const seatId = svg.dataset.seatId;
        var typeRadios = document.getElementsByName('ticketType');
        var ticketType;
        for (var i = 0; i < typeRadios.length; i++) {
            if (typeRadios[i].checked) {
                ticketType = typeRadios[i].value;
            }
        }
        // If already selected, deselect
        if (path.classList.contains('clicked')) {
            path.classList.remove('clicked');

            if (svg.dataset.seatType == SeatType.Handicapped) {
                path.style.fill = '#0000ff';
            } else {
                path.style.fill = '#000000';
            }
            const seatIndex = selectedSeats.indexOf(seatId);
            if (seatIndex > -1) {
                selectedSeats.splice(seatIndex, 1); // Remove seat from selectedSeats
                selectedTicketTypes.splice(seatIndex, 1); // Remove corresponding ticket type
            }
        }
        // If not selected, select
        else {
            if (!ticketType || ticketType === '') {
                alert('Please select a ticket type first.');
                return;
            }
            else {
                path.classList.add('clicked');
                path.style.fill = '#ffffff';

                if (!selectedSeats.includes(seatId)) {
                    selectedSeats.push(seatId);
                    selectedTicketTypes.push(ticketType)
                }
            }
            
        }
    }
};

/**
 * Handles mouse hover events
 */
const onHover = (event) => {
    let svg = null;

    // Check if hovering on valid seat element
    if (event.target.tagName === 'path') {
        svg = event.target.closest('.seat-svg');
    }

    else if (event.target.classList.contains('seat-svg')) {
        svg = event.target;
    }

    else if (event.target.classList.contains('spanContainer')) {
        svg = event.target.querySelector('.seat-svg');
    }
    // Change color on hover
    if (svg && svg.dataset.interactive === 'true') {
        const path = svg.querySelector('path');
        if (path && !path.classList.contains('clicked')) {
            path.style.fill = '#ffffff'; // White on hover
        }
    }
}

/**
 * Handles mouse hover out events
 */
const onHoverOut = (event) => {
    let svg = null;

    // Check if hovered off valid seat element
    if (event.target.tagName === 'path') {
        svg = event.target.closest('.seat-svg');
    }

    else if (event.target.classList.contains('seat-svg')) {
        svg = event.target;
    }

    else if (event.target.classList.contains('spanContainer')) {
        svg = event.target.querySelector('.seat-svg');
    }

    // Restore color based on seat type
    if (svg && svg.dataset.interactive === 'true') {
        const path = svg.querySelector('path');
        if (path && !path.classList.contains('clicked')) {
            if (svg.dataset.seatType == SeatType.Handicapped) {
                path.style.fill = '#0000ff';
            } else {
                path.style.fill = '#000000';
            }
        }
    }
}

/**
 * Gets the currently selected seats
 * @returns {Array} Array of selected seat IDs
 */
function getSelectedSeats() {
    return selectedSeats;
}

function bookSeats() {
    const selectedSeats = getSelectedSeats();

    if (selectedSeats.length === 0) {
        alert('Please select at least one seat.');
        return;
    }

    const form = document.getElementById('booking-form');
    const container = document.getElementById('seat-inputs-container');
    container.innerHTML = '';

    // Add hidden input for each selected seat ID
    selectedSeats.forEach((seatId, index) => {
        const seatInput = document.createElement('input');
        seatInput.type = 'hidden';
        seatInput.name = 'seatID';
        seatInput.value = parseInt(seatId);
        container.appendChild(seatInput);

        const ticketInput = document.createElement('input');
        ticketInput.type = 'hidden';
        ticketInput.name = 'TicketTypes';
        ticketInput.value = selectedTicketTypes[index];
        container.appendChild(ticketInput);
    });

    // Submit the form
    form.submit();
}

// Initialize Seats when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    if (typeof seatData !== 'undefined') {
        buildDynamicGrid(seatData);
    } else {
        console.error('No seat data available');
    }
});

window.addEventListener('mouseover', onHover);
window.addEventListener('mouseout', onHoverOut);
window.addEventListener('click', onClick);