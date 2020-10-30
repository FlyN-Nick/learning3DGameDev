import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';

admin.initializeApp();
const db = admin.firestore();

type timeObj = { "time": number; };
type percentileReqObj = { "level": string; "time": number; }

/**
 * Triggers when a new level playthrough time is added to the database.
 * Will update the level record if it is faster than the old record.
 * Will update the average playthrough time for the level. 
 */
export const levelDataCreated = functions.firestore
    .document('levelData/eachLevel/{levelNum}/{docID}')
    .onCreate((snap, context) => 
    {
        const newLevelTime = snap.data() as timeObj;
        const levelNum = context.params.levelNum as string;
        db.doc(`recordStats/${levelNum}`).get()
            .then((doc) =>
            {
                const oldRecord = doc.data() as timeObj;
                if (newLevelTime.time < oldRecord.time)
                {
                    updateLevelRecord(levelNum, newLevelTime);
                }
            })
            .catch( (err) => { functions.logger.error(err); } );
        updateLevelAverage(levelNum);
    });

/**
 * Triggers when a record is updated.
 * All this does is log to the console.
 * For testing/debugging purposes.
 */
export const recordUpdated = functions.firestore
    .document('recordStats/{levelNum}')
    .onUpdate((change, context) => 
    {
        const newRecord = change.after.data();
        const oldRecord = change.before.data();
        const levelNum = context.params.levelNum;
        functions.logger.log(`Updated record for level #${levelNum} is ${newRecord}s. Previously was ${oldRecord}s.`);
    });

/**
 * HTTPS request for a user's percentile for any playthrough time.
 */
export const getPercentile = functions.https.onRequest((req, res) =>
{
    const percentileReqData: percentileReqObj = req.body || req.query;
    db.collection(`levelData/eachLevel/${percentileReqData.level}`).get()
        .then((data) =>
        {
            const levelTimes: number[] = [];
            data.forEach(doc =>
            {
                const time = doc.data() as timeObj;
                levelTimes.push(time.time);
            });
            res.send({ percentile: percentile(levelTimes, percentileReqData.time) });
        })
        .catch((err) => 
        { 
            functions.logger.error(err); 
            res.sendStatus(500);
        });
})

/** 
 * Calculates percentile given # and array. 
 * @param {number[]} arr - the array
 * @param {number} val - the #
 */
function percentile(arr: number[], val: number)
{
    return arr.reduce( (acc, v) => acc + (v < val ? 1 : 0) + (v === val ? 0.5 : 0), 0 ) / arr.length * 100;
}

/** 
 * Calculates average given array. 
 * @param {number[]} arr - the array
 */
function average(arr: number[])
{
    return arr.reduce((a, b) => (a + b)) / arr.length;
}

/** 
 * Updates a level record. 
 * @param {string} level - the level 
 * @param {timeObj} time - the time in object format
 */
function updateLevelRecord(level: string, time: timeObj)
{
    db.doc(`recordStats/${level}`).set(time)
        .then( (data) => { functions.logger.log(`New record ${time.time}s for level #${level}`); } )
        .catch( (err) => { functions.logger.error(err); } );
}

/**
 * Updates the average level playthrough time.
 * @param level - the level
 */
function updateLevelAverage(level: string)
{
    db.collection(`levelData/eachLevel/${level}`).get()
        .then((data) =>
        {
            const levelTimes: number[] = [];
            data.forEach(doc =>
            {
                const time = doc.data() as timeObj;
                levelTimes.push(time.time);
            });
            const avg = average(levelTimes);
            const avgObj: timeObj = { time: avg };
            db.doc(`levelAverages/${level}`).set(avgObj)
                .then( (_) => { functions.logger.log(`New average ${avg}s for level #${level}`); } )
                .catch( (err) => { functions.logger.error(err); } );
        })
        .catch( (err) => { functions.logger.error(err); } );
}