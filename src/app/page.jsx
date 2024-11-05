'use client'

import { Player } from './components/MediaPlayer' 
import { OpenFolderPickerDialog as openFolderPickerDialog } from './util/FolderPicker';
import './util/FileInteractionUtils'


export default function Home() {
  return (
    
    <div className="flex items-stretch w-screen h-screen">
      <div className="p-5 w-[65%]">
        <h1 className="flex-center text-4xl text-center">Pick a folder</h1>
        <p className="flex-center text-center text-sm">Files Remaining: Idk man select a folder first</p>
        <button className="border-white/20 hover:border-white/10 bg-blue-500 hover:bg-blue-600 px-4 py-1 border-b rounded" onClick={async () => await openFolderPickerDialog()}>YAY!</button>
        <Player></Player>
      </div>
      <div className="p-5 w-[35%]">
        <h1>Audio Tuning</h1>
        <p>Tuning Dropdown here</p>
        <h1>Special Effects</h1>
        <p>Effects boxes here</p>
        <button>Confirm Settings</button>
      </div>
    </div>
  );
}
