// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/statistics")
    .withAutomaticReconnect()
    .build();

// Store phone number statistics
let phoneNumberStats = [];

// Start the connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR connected");
    } catch (err) {
        console.error("SignalR connection error: ", err);
        setTimeout(startConnection, 5000);
    }
}

// Handle account statistics updates
connection.on("ReceiveAccountStatistics", (stats) => {
    document.getElementById("account-rate").textContent = stats.currentMessagesPerSecond.toFixed(1);
    document.getElementById("account-total").textContent = stats.totalMessagesSent;
    document.getElementById("active-numbers").textContent = stats.activePhoneNumbers;

    // Add warning/danger classes based on rate
    const accountRate = document.getElementById("account-rate");
    accountRate.className = "stat-value";

    if (stats.currentMessagesPerSecond >= 15) {
        accountRate.classList.add("rate-danger");
    } else if (stats.currentMessagesPerSecond >= 10) {
        accountRate.classList.add("rate-warning");
    }

    // Add rate limit progress bar
    const ratePercentage = (stats.currentMessagesPerSecond / 20) * 100;
    const progressColor = ratePercentage >= 80 ? '#FF2D55' :
        ratePercentage >= 60 ? '#FF9500' : '#5856D6';

    // Create or update the progress bar
    let progressBar = document.getElementById("account-rate-progress");
    if (!progressBar) {
        const accountRateCard = document.querySelector("#account-rate").closest(".glass-card");
        progressBar = document.createElement("div");
        progressBar.id = "account-rate-progress";
        progressBar.style.marginTop = "15px";
        progressBar.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small style="color: var(--text-secondary);">0</small>
                <small style="color: var(--text-secondary); font-weight: 500;">Rate Limit: 20/sec</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div id="account-rate-bar" style="height: 100%; width: ${Math.min(ratePercentage, 100)}%; background-color: ${progressColor}; border-radius: 4px; transition: width 0.3s ease, background-color 0.3s ease;"></div>
            </div>
        `;
        accountRateCard.appendChild(progressBar);
    } else {
        const progressBarElement = document.getElementById("account-rate-bar");
        progressBarElement.style.width = `${Math.min(ratePercentage, 100)}%`;
        progressBarElement.style.backgroundColor = progressColor;
    }

    // Add placeholder elements to the other boxes to make them the same height
    let totalMessagesPlaceholder = document.getElementById("total-messages-placeholder");
    if (!totalMessagesPlaceholder) {
        const totalMessagesCard = document.querySelector("#account-total").closest(".glass-card");
        totalMessagesPlaceholder = document.createElement("div");
        totalMessagesPlaceholder.id = "total-messages-placeholder";
        totalMessagesPlaceholder.style.marginTop = "15px";
        totalMessagesPlaceholder.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small style="color: var(--text-secondary);">Total count</small>
                <small style="color: var(--text-secondary); font-weight: 500;">All time</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div style="height: 100%; width: 100%; background-color: rgba(88, 86, 214, 0.3); border-radius: 4px;"></div>
            </div>
        `;
        totalMessagesCard.appendChild(totalMessagesPlaceholder);
    }

    let activeNumbersPlaceholder = document.getElementById("active-numbers-placeholder");
    if (!activeNumbersPlaceholder) {
        const activeNumbersCard = document.querySelector("#active-numbers").closest(".glass-card");
        activeNumbersPlaceholder = document.createElement("div");
        activeNumbersPlaceholder.id = "active-numbers-placeholder";
        activeNumbersPlaceholder.style.marginTop = "15px";
        activeNumbersPlaceholder.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 5px;">
                <small style="color: var(--text-secondary);">Active</small>
                <small style="color: var(--text-secondary); font-weight: 500;">Last 60 minutes</small>
            </div>
            <div style="height: 8px; width: 100%; background-color: rgba(0,0,0,0.05); border-radius: 4px; overflow: hidden;">
                <div style="height: 100%; width: 100%; background-color: rgba(88, 86, 214, 0.3); border-radius: 4px;"></div>
            </div>
        `;
        activeNumbersCard.appendChild(activeNumbersPlaceholder);
    }
});

// Handle phone number statistics updates
connection.on("ReceivePhoneNumberStatistics", (stats) => {
    phoneNumberStats = stats;
    applyPhoneNumberFilter();
});

// Filter phone numbers
document.getElementById("phone-filter").addEventListener("input", applyPhoneNumberFilter);

function applyPhoneNumberFilter() {
    const filterValue = document.getElementById("phone-filter").value.toLowerCase();
    const filteredStats = phoneNumberStats.filter(stat =>
        stat.phoneNumber.toLowerCase().includes(filterValue)
    );

    renderPhoneNumberTable(filteredStats);
}

// Render the phone number table
function renderPhoneNumberTable(stats) {
    const tableBody = document.getElementById("phone-numbers-table");
    tableBody.innerHTML = "";

    if (stats.length === 0) {
        const row = document.createElement("tr");
        row.innerHTML = `<td colspan="4" class="text-center">No phone numbers found</td>`;
        tableBody.appendChild(row);
        return;
    }

    // Sort by messages per second (descending)
    stats.sort((a, b) => b.currentMessagesPerSecond - a.currentMessagesPerSecond);

    stats.forEach(stat => {
        const row = document.createElement("tr");

        // Format the last activity date
        const lastActivity = new Date(stat.lastActivity);
        const formattedDate = lastActivity.toLocaleString();

        // Add rate warning/danger classes
        let rateClass = "";
        if (stat.currentMessagesPerSecond >= 4) {
            rateClass = "rate-danger";
        } else if (stat.currentMessagesPerSecond >= 3) {
            rateClass = "rate-warning";
        }

        // Calculate percentage of rate limit used
        const ratePercentage = (stat.currentMessagesPerSecond / 5) * 100;
        const progressColor = ratePercentage >= 80 ? '#FF2D55' :
            ratePercentage >= 60 ? '#FF9500' : '#5856D6';

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
        `;

        tableBody.appendChild(row);
    });
}

// Start the connection when the page loads
document.addEventListener("DOMContentLoaded", startConnection); 