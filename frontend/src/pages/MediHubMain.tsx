import { Outlet } from "react-router-dom";

/**
 * Page default init page (sidebar, topbar)
 * CreatedBy: PQ Huy (21.11.2024)
 */
const MediHubMain = () => {
    return (
        <div className="medihub-main w-full h-full">
            <div className="medi-topbar">
                <div className="medi-topbar__logo"></div>
                <div className="medi-topbar__right">
                    <div className="medi-icon topbar__right--setting"></div>
                    <div className="topbar__right--notification"></div>
                    <div className="topbar__right--user"></div>
                </div>
            </div>
            <div className="medi-content felx">
                <div className="medi-content__detail">
                    <Outlet/>
                </div>
            </div>
        </div>
    )
}

export default MediHubMain;