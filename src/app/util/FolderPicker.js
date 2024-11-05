import { open } from '@tauri-apps/plugin-dialog'
import { setFolderPath, setData } from './FileInteractionUtils'

export async function OpenFolderPickerDialog()
{
    const folder = await open({
        multiple: false,
        directory: true
    })

    alert(folder)
    await setFolderPath(folder)
    await setData()
}