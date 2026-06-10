const mysql = require("mysql2");
/*opçao de coneccçao*/
const connectionOptions = {
    host: "localhost",
    user: "root",
    password: "1234",
    database: "sys"
};

function getConnection() {
    return mysql.createPool(connectionOptions).promise();
}
/*funçoes post e get para mexer com a base de dados*/
async function updateRound(req, res) {
    const { id, round } = req.body;

    if (!id || !round) {
        return res.status(400).send("ID and round are required");
    }

    try {
        const connection = getConnection();
        
       
        const [results] = await connection.query("SELECT highest_round FROM player_accounts WHERE id = ?", [id]);
        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }
        const currentHighestRound = results[0].highest_round;

       
        await connection.query("UPDATE player_accounts SET round = ?, highest_round = ? WHERE id = ?", [
            round, 
            Math.max(round, currentHighestRound), 
            id
        ]);

        console.log(`Round updated for player with ID: ${id}, current round: ${round}, highest round: ${Math.max(round, currentHighestRound)}`);
        res.sendStatus(200);
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}
async function updateDeaths(req, res) {
    const { id } = req.body;

    if (!id) {
        return res.status(400).send("ID is required");
    }

    try {
        const connection = getConnection();

        
        const [results] = await connection.query("SELECT deaths FROM player_accounts WHERE id = ?", [id]);
        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }
        const currentDeaths = results[0].deaths;

       
        await connection.query("UPDATE player_accounts SET deaths = ? WHERE id = ?", [
            currentDeaths + 1,
            id
        ]);

        console.log(`Deaths updated for player with ID: ${id}, new deaths count: ${currentDeaths + 1}`);
        res.sendStatus(200);
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}

async function getRound(req, res) {
    const playerId = req.query.id;

    if (!playerId) {
        return res.status(400).send("Player ID is required");
    }

    try {
        const connection = getConnection();
        const [results] = await connection.query("SELECT round FROM player_accounts WHERE id = ?", [playerId]);

        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }

        res.json({ round: results[0].round });
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}


async function updateKills(req, res) {
    const { id, kills } = req.body;

    if (!id || kills == null) {
        return res.status(400).send("ID and kills are required");
    }

    try {
        const connection = getConnection();

        
        const [results] = await connection.query("SELECT enemies_killed FROM player_accounts WHERE id = ?", [id]);
        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }
        const currentKills = results[0].enemies_killed;

       
        await connection.query("UPDATE player_accounts SET enemies_killed = ? WHERE id = ?", [
            currentKills + kills,
            id
        ]);

        console.log(`Kills updated for player with ID: ${id}, current kills: ${currentKills + kills}`);
        res.sendStatus(200);
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}

async function updateTimesPlayed(req, res) {
    const { id } = req.body;

    if (!id) {
        return res.status(400).send("ID is required");
    }

    try {
        const connection = getConnection();

       
        const [results] = await connection.query("SELECT times_played FROM player_accounts WHERE id = ?", [id]);
        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }
        const currentTimesPlayed = results[0].times_played;

        
        await connection.query("UPDATE player_accounts SET times_played = ? WHERE id = ?", [
            currentTimesPlayed + 1,
            id
        ]);

        console.log(`Times played updated for player with ID: ${id}, new times played: ${currentTimesPlayed + 1}`);
        res.sendStatus(200);
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}

async function loginUser(req, res) {
    const { username, password } = req.body;

    if (!username || !password) {
        return res.status(400).send("Username and password are required");
    }

    try {
        const connection = getConnection();
        const [results] = await connection.query("SELECT * FROM player_accounts WHERE username = ? AND password = ?", [username, password]);

        if (results.length === 0) {
            return res.status(401).send("Invalid username or password");
        }

        const user = results[0];
        res.json({ id: user.id, username: user.username, round: user.round });
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}
async function getPlayerData(req, res) {
    const playerId = req.query.id;

    if (!playerId) {
        return res.status(400).send("Player ID is required");
    }

    try {
        const connection = getConnection();
        const [results] = await connection.query(
            `SELECT round, enemies_killed, times_played, deaths, highest_round 
             FROM player_accounts 
             WHERE id = ?`,
            [playerId]
        );

        if (results.length === 0) {
            return res.status(404).send("Player not found");
        }

        const playerData = results[0];
        res.json(playerData);
    } catch (err) {
        console.error("Error:", err.message);
        res.sendStatus(500);
    }
}

module.exports = {
    updateRound,
    getRound,
    updateKills,
    updateTimesPlayed,
    updateDeaths,
    getPlayerData,
    loginUser
};
