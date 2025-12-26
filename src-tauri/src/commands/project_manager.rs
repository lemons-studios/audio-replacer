use std::{fs, path::Path, str, vec};
use rand::seq::SliceRandom;
use tauri::command;
use walkdir::WalkDir;

#[command]
pub fn get_all_files(path: &str) -> Vec<String> {
    let mut files = vec![];
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_file() {
            files.push(e.path().display().to_string());
        }
    }
    files.sort(); // Just sort them in alphabetical order
    files
}

#[command]
pub fn get_all_directories(path: &str) -> Vec<String> {
    let mut dirs = vec![];
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_dir() {
            if e.depth() == 0 {
                continue;
            }
            dirs.push(e.path().display().to_string());
        }
    }
    dirs.sort();
    dirs
}

#[command]
pub fn delete_empty_subdirectories(project_path: &str) {
    let path = Path::new(project_path);
    if !path.is_dir() {
        return;
    }
    let dirs: Vec<_> = WalkDir::new(path)
        .contents_first(true)
        .into_iter()
        .filter_map(|res| match res {
            Ok(entry) => Some(entry),
            Err(_e) => None,
        })
        .filter(|e| e.file_type().is_dir())
        .collect();

    for e in dirs {
        let dir_path = e.path();

        match fs::read_dir(dir_path) {
            Ok(mut read_dir) => {
                if read_dir.next().is_none() {
                    if let Err(e) = fs::remove_dir(dir_path) {
                        eprintln!("Failed to delete directory {:?}: {}", dir_path, e);
                    }
                }
            }
            Err(e) => {
                eprintln!("Could not read dir {:?}: {}", dir_path, e);
            }
        }
    }
}

#[command]
pub fn randomize_file_order(mut arr: Vec<String>) -> Vec<String> {
    let mut rng = rand::rng();
    arr.shuffle(&mut rng);
    arr
}
