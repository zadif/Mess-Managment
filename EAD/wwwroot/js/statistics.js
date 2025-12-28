document.addEventListener("DOMContentLoaded", function () {

    // --- 1. SweetAlert2 Implementation for Errors ---
    // Accessing the data from the bridge
    if (dashboardData.serverError && dashboardData.serverError !== "") {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: dashboardData.serverError,
            background: '#2b2b40',
            color: '#fff',
            confirmButtonColor: '#d35400'
        });
    }

    // --- 2. Chart.js Global Config for Dark Theme ---
    Chart.defaults.color = '#a0a0a0';
    Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';

    // --- 3. Data Calculation ---
    // We use the 'dashboardData' object here instead of @Model
    var paidBills = dashboardData.paidBills;
    var unpaidBills = dashboardData.totalBills - paidBills;

    var activeUsers = dashboardData.totalUsers - dashboardData.inactiveUsers;
    var inactiveUsers = dashboardData.inactiveUsers;

    // --- 4. Render Bills Chart (Doughnut) ---
    const billsCanvas = document.getElementById('billsChart');

    if (billsCanvas) {
        var ctxBills = billsCanvas.getContext('2d');
        new Chart(ctxBills, {
            type: 'doughnut',
            data: {
                labels: ['Paid Bills', 'Unpaid Bills'],
                datasets: [{
                    data: [paidBills, unpaidBills],
                    backgroundColor: [
                        '#2ecc71', // Green
                        '#e74c3c'  // Red
                    ],
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 20 }
                    }
                }
            }
        });
    }

    // --- 5. Render Users Chart (Pie) ---
    const usersCanvas = document.getElementById('usersChart');

    if (usersCanvas) {
        var ctxUsers = usersCanvas.getContext('2d');
        new Chart(ctxUsers, {
            type: 'pie',
            data: {
                labels: ['Active Users', 'Inactive Users'],
                datasets: [{
                    data: [activeUsers, inactiveUsers],
                    backgroundColor: [
                        '#3498db', // Blue
                        '#95a5a6'  // Grey
                    ],
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 20 }
                    }
                }
            }
        });
    }
});