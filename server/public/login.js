document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    loginForm.addEventListener('submit', async function(event) {
        event.preventDefault();

        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        try {
            const response = await fetch('/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            if (response.ok) {
                const data = await response.json();
                const userId = data.id;
                const userUsername = data.username;
                const userRound = data.round;

                
                document.getElementById('userId').textContent = userId;
                document.getElementById('userUsername').textContent = userUsername;
                document.getElementById('userRound').textContent = userRound;

                
                const playerDataResponse = await fetch(`/playerData?id=${userId}`);
                if (playerDataResponse.ok) {
                    const playerData = await playerDataResponse.json();
                    document.getElementById('userEnemiesKilled').textContent = playerData.enemies_killed;
                    document.getElementById('userTimesPlayed').textContent = playerData.times_played;
                    document.getElementById('userDeaths').textContent = playerData.deaths;
                    document.getElementById('userHighestRound').textContent = playerData.highest_round;
                } else {
                    console.error('Failed to fetch player data');
                }

                document.getElementById('userInfo').classList.remove('hidden');
            } else {
                console.error('Login failed');
            }
        } catch (error) {
            console.error('Error:', error);
        }
    });
});
