mod project_file_utils
{
    use walkdir::WalkDir;
    use std::str;
    use std::cmp;

    pub static mut project_path : String = String::new();
    pub static mut output_path : String = String::new();

    #[tauri::command]
    pub fn set_project_data(path: &String)
    {
        unsafe 
        {
            project_path = path.to_string();
        }
    }

    #[tauri::command]
    pub fn count_files(path: &str) -> i32
    {
        WalkDir::new(path)
            .into_iter()
            .filter_map(|e| e.ok())
            .filter(|e| e.file_type().is_file())
            .count() as i32
    }
    
    #[tauri::command]
    pub fn get_completion_percentage() -> f32 
    {
        let output_files = count_files("tmp") as f32;
        let input_files = count_files("tmp") as f32;

        output_files / (output_files + input_files)
    }

    #[tauri::command]
    // I don't see a use case where I would ever need to split a directory more than 64 levels up
    pub fn truncate_directory(path: &str, split_levels: &i8) -> String
    {
        let delimiter = "/";
        let mut split: Vec<&str> = path.split(delimiter).collect();
        split.shrink_to(split.len() - (cmp::max(0, *split_levels as usize)));
        split.join(delimiter)
    }
    


}