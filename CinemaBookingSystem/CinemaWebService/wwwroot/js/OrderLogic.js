const timerElement = document.getElementById("timer");
let interval;

function updateTimer() {
    const now = new Date().getTime();
    // Check how much time has passed since the reservation started
    const timeLeft = Math.floor((expirationTime - now) / 1000); // milliseconds to seconds

    if (timeLeft <= 0) {
        // Time's up
        timerElement.textContent = "Reservation expired";
        clearInterval(interval);
        alert("Your seat reservation expired.");
        window.location.href = "/Home/Index";
        return;
    }

    // Separate into minutes and seconds
    let minutes = Math.floor(timeLeft / 60);
    let seconds = timeLeft % 60;

    timerElement.textContent = `${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
}

updateTimer();
// Update every second
interval = setInterval(updateTimer, 1000);