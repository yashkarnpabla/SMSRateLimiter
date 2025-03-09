// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder() // Create a new SignalR connection builder
    .withUrl("/hubs/statistics") // Set the URL to the statistics hub endpoint defined in Program.cs
    .withAutomaticReconnect() // Enable automatic reconnection if the connection is lost
    .build(); // Build the connection

// Store phone number statistics
let phoneNumberStats = []; // Initialize an empty array to store phone number statistics received from the server

// Start the connection
async function startConnection() { // Async function to establish the SignalR connection
    try {
        await connection.start(); // Attempt to start the connection asynchronously
        console.log("SignalR connected"); // Log success message when connected
    } catch (err) {
        console.error("SignalR connection error: ", err); // Log error if connection fails
        setTimeout(startConnection, 5000); // Retry connection after 5 seconds if it fails
    }
}

// Handle account statistics updates
connection.on("ReceiveAccountStatistics", (stats) => { // Register handler for the ReceiveAccountStatistics event from the hub
    document.getElementById("account-rate").textContent = stats.currentMessagesPerSecond.toFixed(1); // Update account rate display with 1 decimal place
    document.getElementById("account-total").textContent = stats.totalMessagesSent; // Update total messages count
    document.getElementById("active-numbers").textContent = stats.activePhoneNumbers; // Update active phone numbers count

    // Add warning/danger classes based on rate
    const accountRate = document.getElementById("account-rate"); // Get the account rate element
    accountRate.className = "stat-value"; // Reset the class to the base stat-value

    if (stats.currentMessagesPerSecond >= 15) { // If rate is 75% or more of the limit (20/sec)
        accountRate.classList.add("rate-danger"); // Add danger class for high rates
    } else if (stats.currentMessagesPerSecond >= 10) { // If rate is 50% or more of the limit
        accountRate.classList.add("rate-warning"); // Add warning class for medium rates
    }

    // Add rate limit progress bar
    const ratePercentage = (stats.currentMessagesPerSecond / 20) * 100; // Calculate percentage of max rate (20/sec)
    const progressColor = ratePercentage >= 80 ? '#FF2D55' : // Red for ≥80%
        ratePercentage >= 60 ? '#FF9500' : '#5856D6'; // Orange for ≥60%, purple for <60%

    // Create or update the progress bar
    let progressBar = document.getElementById("account-rate-progress"); // Try to get existing progress bar
    if (!progressBar) { // If progress bar doesn't exist yet
        const accountRateCard = document.querySelector("#account-rate").closest(".glass-card"); // Find the parent card
        progressBar = document.createElement("div"); // Create a new div for the progress bar
        progressBar.id = "account-rate-progress"; // Set the ID
        progressBar.style.marginTop = "15px"; // Add some margin
        progressBar.innerHTML = ` 
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small id="account-rate-min" style="color: var(--text-secondary);">${stats.currentMessagesPerSecond.toFixed(1)}</small>
                <small style="color: var(--text-secondary); font-weight: 500;">Rate Limit: 20/sec</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div id="account-rate-bar" style="height: 100%; width: ${Math.min(ratePercentage, 100)}%; background-color: ${progressColor}; border-radius: 4px; transition: width 0.3s ease, background-color 0.3s ease;"></div>
            </div>
        `; // Create HTML for the progress bar with current rate and limit
        accountRateCard.appendChild(progressBar); // Add the progress bar to the card
    } else { // If progress bar already exists
        const progressBarElement = document.getElementById("account-rate-bar"); // Get the bar element
        progressBarElement.style.width = `${Math.min(ratePercentage, 100)}%`; // Update the width based on percentage
        progressBarElement.style.backgroundColor = progressColor; // Update the color based on percentage

        // Update the minimum value text to match the current rate
        const minRateElement = document.getElementById("account-rate-min"); // Get the min rate element
        if (minRateElement) {
            minRateElement.textContent = stats.currentMessagesPerSecond.toFixed(1); // Update the text
        }
    }

    // Add placeholder elements to the other boxes to make them the same height
    let totalMessagesPlaceholder = document.getElementById("total-messages-placeholder"); // Try to get existing placeholder
    if (!totalMessagesPlaceholder) { // If placeholder doesn't exist yet
        const totalMessagesCard = document.querySelector("#account-total").closest(".glass-card"); // Find the parent card
        totalMessagesPlaceholder = document.createElement("div"); // Create a new div
        totalMessagesPlaceholder.id = "total-messages-placeholder"; // Set the ID
        totalMessagesPlaceholder.style.marginTop = "15px"; // Add some margin
        totalMessagesPlaceholder.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small style="color: var(--text-secondary);">Total count</small>
                <small style="color: var(--text-secondary); font-weight: 500;">All time</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div style="height: 100%; width: 100%; background-color: rgba(88, 86, 214, 0.3); border-radius: 4px;"></div>
            </div>
        `; // Create HTML for the placeholder with static content
        totalMessagesCard.appendChild(totalMessagesPlaceholder); // Add the placeholder to the card
    }

    let activeNumbersPlaceholder = document.getElementById("active-numbers-placeholder"); // Try to get existing placeholder
    if (!activeNumbersPlaceholder) { // If placeholder doesn't exist yet
        const activeNumbersCard = document.querySelector("#active-numbers").closest(".glass-card"); // Find the parent card
        activeNumbersPlaceholder = document.createElement("div"); // Create a new div
        activeNumbersPlaceholder.id = "active-numbers-placeholder"; // Set the ID
        activeNumbersPlaceholder.style.marginTop = "15px"; // Add some margin
        activeNumbersPlaceholder.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small style="color: var(--text-secondary);">Active</small>
                <small style="color: var(--text-secondary); font-weight: 500;">Last 60 minutes</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div style="height: 100%; width: 100%; background-color: rgba(88, 86, 214, 0.3); border-radius: 4px;"></div>
            </div>
        `; // Create HTML for the placeholder with static content
        activeNumbersCard.appendChild(activeNumbersPlaceholder); // Add the placeholder to the card
    }
});

// Handle phone number statistics updates
connection.on("ReceivePhoneNumberStatistics", (stats) => { // Register handler for the ReceivePhoneNumberStatistics event from the hub
    phoneNumberStats = stats; // Update the global phone number statistics array
    applyPhoneNumberFilter(); // Apply any active filter and render the table
});

// Filter phone numbers
document.getElementById("phone-filter").addEventListener("input", applyPhoneNumberFilter); // Add input event listener to the filter input

function applyPhoneNumberFilter() { // Function to filter phone numbers based on user input
    const filterValue = document.getElementById("phone-filter").value.toLowerCase(); // Get the filter value and convert to lowercase
    const filteredStats = phoneNumberStats.filter(stat =>
        stat.phoneNumber.toLowerCase().includes(filterValue) // Filter stats that include the filter value in the phone number
    );

    renderPhoneNumberTable(filteredStats); // Render the table with filtered statistics
}

// Render the phone number table
function renderPhoneNumberTable(stats) { // Function to render the phone number statistics table
    const tableBody = document.getElementById("phone-numbers-table"); // Get the table body element
    tableBody.innerHTML = ""; // Clear the existing table content

    if (stats.length === 0) { // If no statistics match the filter
        const row = document.createElement("tr"); // Create a new row
        row.innerHTML = `<td colspan="4" class="text-center">No phone numbers found</td>`; // Show "no results" message
        tableBody.appendChild(row); // Add the row to the table
        return; // Exit the function
    }

    // Sort by messages per second (descending)
    stats.sort((a, b) => b.currentMessagesPerSecond - a.currentMessagesPerSecond); // Sort with highest rates first

    stats.forEach(stat => { // Loop through each statistic
        const row = document.createElement("tr"); // Create a new row

        // Format the last activity date
        const lastActivity = new Date(stat.lastActivity); // Convert the timestamp to a Date object
        const formattedDate = lastActivity.toLocaleString(); // Format the date for display

        // Add rate warning/danger classes
        let rateClass = ""; // Initialize rate class as empty
        if (stat.currentMessagesPerSecond >= 4) { // If rate is 80% or more of the limit (5/sec)
            rateClass = "rate-danger"; // Add danger class for high rates
        } else if (stat.currentMessagesPerSecond >= 3) { // If rate is 60% or more of the limit
            rateClass = "rate-warning"; // Add warning class for medium rates
        }

        // Calculate percentage of rate limit used
        const ratePercentage = (stat.currentMessagesPerSecond / 5) * 100; // Calculate percentage of max rate (5/sec)
        const progressColor = ratePercentage >= 80 ? '#FF2D55' : // Red for ≥80%
            ratePercentage >= 60 ? '#FF9500' : '#5856D6'; // Orange for ≥60%, purple for <60%

        row.innerHTML = `
            <td>${stat.phoneNumber}</td>
            <td>
                <div class="${rateClass}" style="display: flex; align-items: center; gap: 10px;">
                    <span>${stat.currentMessagesPerSecond.toFixed(1)}</span>
                    <div style="flex-grow: 1; max-width: 150px;">
                        <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                            <div style="height: 100%; width: ${Math.min(ratePercentage, 100)}%; background-color: ${progressColor}; border-radius: 4px;"></div>
                        </div>
                    </div>
                    <small style="color: var(--text-secondary);">/ 5</small>
                </div>
            </td>
            <td>${stat.totalMessagesSent}</td>
            <td>${formattedDate}</td>
        `; // Create HTML for the row with phone number, rate with progress bar, total messages, and last activity

        tableBody.appendChild(row); // Add the row to the table
    });
}

// Start the connection when the page loads
document.addEventListener("DOMContentLoaded", startConnection); // Add event listener to start the connection when the DOM is loaded