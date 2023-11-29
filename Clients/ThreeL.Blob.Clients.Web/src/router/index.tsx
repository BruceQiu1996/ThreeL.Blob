import React, { lazy } from "react";
import Home from "@/views/Home";
import Login from "@/views/Login";
const Page_UserManage = lazy(() => import("@/views/UserPages/UserManagePage"));
const Page_Upload = lazy(() => import("@/views/FileManagePages/Upload"));
const Page_Download = lazy(() => import("@/views/FileManagePages/Download"));
import { Navigate, RouteObject } from "react-router-dom";

const withLoadingComponent = (Component: JSX.Element) => (
    <React.Suspense fallback={<div>Loading...</div>}>
        {Component}
    </React.Suspense>
);
const routes: RouteObject[] = [
    {
        path: "/",
        element: <Navigate to="/usersManage" />,
    },
    {
        path: "/login",
        element: <Login />
    },
    {
        path: "/",
        element: <Home />,
        children: [
            {
                path: "/usersManage",
                element: withLoadingComponent(<Page_UserManage />),
            },
            {
                path: "/filesManage",
                element: <Navigate to="/filesManage/upload" />,
            },
            {
                path: "/filesManage/upload",
                element: withLoadingComponent(<Page_Upload />),
            },
            {
                path: "/filesManage/download",
                element: withLoadingComponent(<Page_Download />),
            }
        ]
    }
]

export default routes;