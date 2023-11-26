import { legacy_createStore, combineReducers } from 'redux'
import rootReducer from './reducer'
import numReducer from './NumStatus/reducer.ts'
import createNewUserReducer from './CreateNewUserStatus/reducer.ts'

const reducers = combineReducers({ numReducer, createNewUserReducer });
const store = legacy_createStore(reducers)

export default store