import { invoke } from "@tauri-apps/api/core";

// Writing here because I forget this a LOT: DETAILS are the first line of text, STATE is the second. No clue why it trips me up this bad
let currentDetails = "Test Details";
let currentState = "Test State";
let startTime: number | undefined = undefined;

/**
 * @description (Re)Initializes the Discord rich presence connection, and (effectively) gets the app launch time of startTime is undefined
 */
export function startRichPresence() {
    if(startTime === undefined) {
        startTime = Date.now();
    }

    invoke('start_discord_rpc', {
        details: currentDetails,
        state: currentState,
        startTime: startTime
    });
}

/**
 * @description Updates the details (first small text) and re-initializes the rich presence
 * @param newState New details (first small text) value
 */
export function setDetails(newDetails: string) {
    currentDetails = newDetails;
    startRichPresence();
}

/**
 * @description Updates the state (second small text) and re-initializes the rich presence
 * @param newState New state (second small text) value
 */
export function setState(newState: string) {
    currentState = newState;
    startRichPresence();
}
