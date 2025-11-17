// As a general rule of thumb, all tauri commands written in rust for this project
// Will be for more computationally expensive functions that would benefit
// From rust's extra performance when compared to TypeScript

pub mod app_functions;
pub mod project_manager;
pub mod whisper_utils;
pub mod discord_rpc;