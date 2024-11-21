import { Outlet } from "react-router-dom";

/**
 * Page default init page (sidebar, topbar)
 * CreatedBy: PQ Huy (21.11.2024)
 */
const MediHubMain = () => {
    return (
        <div className="w-full h-fulll">
            <div className="medi-topbar">
                
            </div>
            <div className="medi-content felx">
                <div className="medi-content__sidebar">
                    content__sidebar
                </div>
                <div className="medi-content__detail">
                    <Outlet/>
                </div>
            </div>
        </div>
    )
}

export default MediHubMain;