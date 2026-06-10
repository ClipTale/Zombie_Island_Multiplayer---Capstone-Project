const express = require('express');
const hbs = require('express-handlebars');
const path = require('path');
const app = express();
const requestHandlers = require('./request-handlers');


const hbsConfig = hbs.create({
    extname: 'handlebars',
    defaultLayout: 'main',
    layoutsDir: path.join(__dirname, 'views', 'layouts'),
    partialsDir: path.join(__dirname, 'views', 'partials'),
});

app.engine('handlebars', hbsConfig.engine);
app.set('view engine', 'handlebars');


app.use(express.static(path.join(__dirname, 'public')));
app.use(express.json());


app.get('/playerData', requestHandlers.getPlayerData);
app.post('/updateDeaths', requestHandlers.updateDeaths);
app.post('/updateRound', requestHandlers.updateRound);
app.post('/updateKills', requestHandlers.updateKills);
app.get('/getRound', requestHandlers.getRound);
app.post('/login', requestHandlers.loginUser);
app.post('/updateTimesPlayed', requestHandlers.updateTimesPlayed);


app.get('/', (req, res) => {
    res.render('index', { title: 'Home' });
});

/*coloca o server a escuta*/ 
app.listen(6969, function() {
    console.log("Server running on port 6969");
});
