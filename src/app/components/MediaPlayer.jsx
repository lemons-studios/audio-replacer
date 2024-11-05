
export function Player()
{
    return(
        <div className="flex justify-center items-center text-gray-300">
          <div className="flex items-center gap-3 bg-gray-700 shadow-lg pr-4 pl-2 rounded-full w-96 h-12">
            <button className="flex justify-center items-center bg-gray-600 hover:bg-gray-500 shadow-md rounded-full w-8 h-8 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="1em" height="1em" className="main-grid-item-icon" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2">
                <polygon points="5 3 19 12 5 21 5 3" />
              </svg>
            </button>
      
            <p>0:20</p>
      
            <div className="flex justify-stretch items-center h-4 group/bar grow">
              <div className="relative bg-gray-600 rounded-full h-1 transition-all grow">
                <div className="top-0 bottom-0 left-0 absolute bg-gray-500 rounded-full h-full" style={{width: `${20/60}%`}}></div>
                <div className="top-1/2 absolute bg-gray-400 opacity-0 group-hover/bar:opacity-100 shadow-md rounded-full w-3 h-3 transition-opacity -translate-x-1/2 -translate-y-1/2" style={{left: `${20/60}`}}></div>
              </div>
            </div>
          </div>
        </div>
    );
}