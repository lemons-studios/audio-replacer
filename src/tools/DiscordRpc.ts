import { Client, ActivityType } from 'minimal-discord-rpc';

let client: Client;

export async function startRichPresence() {
  client = new Client({
    clientId: '',
  });

  client.on('ready', () => {

  })
}

export async function setDetails(newDetails: string) {

}

export async function setState(newState: string) {
}

export async function setLargeImage(key: string) {

}

export async function setLargeImageText(text: string) {
}

export async function setSmallImage(key: string) {
}

export async function setSmallImageText(text: string) {
}

export async function stopRichPresence() {

}
