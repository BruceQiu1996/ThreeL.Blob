const store: CommonStore = {
    state: {
        userName: null,
        password: null,
        confirmPassword: null,
    },
    actions: {
        updateCreateNewUserUserName(newState: { userName: string, password: string, confirmPassword: string }, action: { val: string }) {
            newState.userName = action.val;
        },
        updateCreateNewUserPassword(newState: { userName: string, password: string, confirmPassword: string }, action: { val: string}) {
            newState.password = action.val;
        },
        updateCreateNewUserConfirmPassword(newState: { userName: string, password: string, confirmPassword: string }, action: { val: string }) {
            newState.confirmPassword = action.val;
        },
    },

    actionNames: {}
}

for (let key in store.actions) {
    store.actionNames[key] = key
}

export default store
