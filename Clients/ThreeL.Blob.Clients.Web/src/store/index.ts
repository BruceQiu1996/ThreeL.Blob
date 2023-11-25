import { legacy_createStore, combineReducers } from 'redux'
import rootReducer from './reducer'
import numReducer from './NumStatus/reducer.ts'
import loginUserReducer from './LoginUserStatus/reducer.ts'

const reducers = combineReducers({ numReducer, loginUserReducer });
const store = legacy_createStore(reducers)

export default store