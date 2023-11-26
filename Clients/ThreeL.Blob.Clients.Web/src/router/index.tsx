import React, { lazy } from "react";
import Home from "@/views/Home";
import Login from "@/views/Login";
const Page_UserManage = lazy(() => import("@/views/UserPages/UserManagePage"));
import { Navigate, RouteObject } from "react-router-dom";

const withLoadingComponent = (Component: JSX.Element) => (
    <React.Suspense fallback={<div>Loading...</div>}>
        {Component}
    </React.Suspense>
);
const routes : RouteObject[] = [
    {
        path: "/",
        element: <Navigate to="/UserManage" />,
    },
    {
        path: "/login",
        element: <Login/>
    },
    {
        path: "/",
        element: <Home />,
        children: [
            {
                path: "/UserManage",
                element: withLoadingComponent(<Page_UserManage />),
            }
        ]
    }
]

export default routes;