const store: CommonStore = {
    state: {
        id: null,
        userName: null,
        role: null,
        accessToken: null,
    },
    actions: {
        UpdateLoginUser(newState: { id: number, userName: string, role: string, accessToken: string }, action: { val: { id: number, userName: string, role: string, accessToken: string } }) {
            console.log('UpdateLoginUser', action.val)
            newState.id = action.val.id;
            newState.userName = action.val.userName;
            newState.role = action.val.role;
            newState.accessToken = action.val.accessToken;
        }
    },
    actionNames: {}
}

for (let key in store.actions) {
    store.actionNames[key] = key
}

export default store
