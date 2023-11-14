import React, { lazy } from "react";
import Home from "@/views/Home";
import Login from "@/views/Login";
const Page_1 = lazy(() => import("@/views/page1"));
const Page_2 = lazy(() => import("@/views/page2"));
import { Navigate } from "react-router-dom";

const withLoadingComponent = (Component: JSX.Element) => (
    <React.Suspense fallback={<div>Loading...</div>}>
        {Component}
    </React.Suspense>
);
const routes = [
    {
        path: "/",
        element: <Navigate to="/page1" />,
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
                path: "/page1",
                element: withLoadingComponent(<Page_1 />),
            },
            {
                path: "/page2",
                element: withLoadingComponent(<Page_2 />),
            }
        ]
    }
]

export default routes;