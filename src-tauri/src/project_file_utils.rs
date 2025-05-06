mod project_file_utils
{
    use walkdir::WalkDir;
    
    pub unsafe fn set_project_data(path: &str)
    {
        // project_path = path.parse::<String>().unwrap()
    }

    pub fn count_files(path: &str) -> i32
    {
        WalkDir::new(path)
            .into_iter()
            .filter_map(|e| e.ok())
            .filter(|e| e.file_type().is_file())
            .count() as i32
    }
    
    pub fn get_completion_percentage() -> f32 
    {
        // Type casting my beloved. TODO: Get the actual path contents
        let output_files = count_files("tmp") as f32;
        let input_files = count_files("tmp") as f32;
        
        output_files / (output_files + input_files)
    }
    
    
}